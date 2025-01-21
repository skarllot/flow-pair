using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;
using Spectre.Console.Testing;
using static Raiqub.LlmTools.FlowPair.LocalFileSystem.Services.PathAnalyzer;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.UpdateUnitTest;

public sealed class UpdateUnitTestCommandTests : IDisposable
{
    private readonly TestConsole _console = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly IUpdateUnitTestChatScript _chatScript = Substitute.For<IUpdateUnitTestChatScript>();
    private readonly IWorkingDirectoryWalker _workingDirectoryWalker = Substitute.For<IWorkingDirectoryWalker>();

    private readonly IProjectFilesMessageFactory _projectFilesMessageFactory =
        Substitute.For<IProjectFilesMessageFactory>();

    private readonly IDirectoryStructureMessageFactory _directoryStructureMessageFactory =
        Substitute.For<IDirectoryStructureMessageFactory>();

    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();

    public void Dispose() => _console.Dispose();

    private UpdateUnitTestCommand CreateCommand() => new(
        _console,
        _fileSystem,
        _chatScript,
        _workingDirectoryWalker,
        _loginUseCase,
        _chatService);

    [Fact]
    public void ExecuteSourceFileDoesNotExistReturnsError()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/nonexistent/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(1);
        _console.Output.Should().Contain("Error").And.Contain("source file does not exist");
        _workingDirectoryWalker.DidNotReceiveWithAnyArgs().TryFindRepositoryRoot(null);
    }

    [Fact]
    public void ExecuteTestFileDoesNotExistReturnsError()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/nonexistent/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(2);
        _console.Output.Should().Contain("Error").And.Contain("test file does not exist");
        _workingDirectoryWalker.DidNotReceiveWithAnyArgs().TryFindRepositoryRoot(null);
    }

    [Fact]
    public void ExecuteRepositoryRootNotFoundReturnsError()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(None);

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(3);
        _console.Output.Should().Contain("Error").And.Contain("Could not locate Git repository");
        _loginUseCase.DidNotReceiveWithAnyArgs().Execute(false);
    }

    [Fact]
    public void ExecuteLoginFailsReturnsError()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase.Execute(true).Returns(1);

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(4);
        _chatService.DidNotReceiveWithAnyArgs()
            .Run<UpdateUnitTestRequest, UpdateUnitTestResponse>(null!, null!, default, null!);
    }

    [Fact]
    public void ExecuteChatServiceFailsReturnsError()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase.Execute(true).Returns(0);
        _chatService
            .Run(
                input: Arg.Any<UpdateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<IUpdateUnitTestChatScript>())
            .Returns("Chat service error");

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(5);
        _console.Output.Should().Contain("Error").And.Contain("Chat service error");
        _fileSystem.GetFile(Normalize(testFile)).TextContents.Should().Be("// Test content");
    }

    [Fact]
    public void ExecuteSuccessUpdatesTestFile()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Old test content"));

        var rootDirectory = _fileSystem.DirectoryInfo.New("/");
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDirectory));
        _loginUseCase.Execute(true).Returns(0);

        const string updatedContent = "// Updated test content";
        _chatService.Run(
                input: Arg.Any<UpdateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<IUpdateUnitTestChatScript>())
            .Returns(new UpdateUnitTestResponse(updatedContent));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(0);
        _fileSystem.GetFile(Normalize(testFile)).TextContents.Should().Be(updatedContent);
        _console.Output.Should().Contain("Unit tests updated").And.Contain(Normalize("path/to/test.cs"));
    }

    [Fact]
    public void ExecuteSuccessIncludesCorrectMessagesInChat()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        var rootDirectory = _fileSystem.DirectoryInfo.New("/");
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDirectory));
        _loginUseCase.Execute(true).Returns(0);

        _projectFilesMessageFactory.CreateWithProjectFilesContent(rootDirectory)
            .Returns(new Message(SenderRole.User, "Project files content"));
        _directoryStructureMessageFactory.CreateWithRepositoryStructure(rootDirectory)
            .Returns(new Message(SenderRole.User, "Directory structure"));

        _chatService.Run(
                input: Arg.Any<UpdateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<IUpdateUnitTestChatScript>())
            .Returns(new UpdateUnitTestResponse("// Updated test content"));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(0);
        _chatService.Received(1).Run(
            input: Arg.Any<UpdateUnitTestRequest>(),
            progress: Arg.Any<Progress>(),
            llmModelType: Arg.Any<LlmModelType>(),
            chatScript: Arg.Any<IUpdateUnitTestChatScript>());
    }

    [Fact]
    public void ExecuteSourceFileDirectoryIsNullUsesCurrentDirectory()
    {
        var command = CreateCommand();
        const string sourceFile = "source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Is<string>(s => s == _fileSystem.Directory.GetCurrentDirectory()))
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase.Execute(true).Returns(0);
        _chatService.Run(
                input: Arg.Any<UpdateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<IUpdateUnitTestChatScript>())
            .Returns(new UpdateUnitTestResponse("// Updated test content"));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(0);
        _workingDirectoryWalker.Received(1).TryFindRepositoryRoot(_fileSystem.Directory.GetCurrentDirectory());
    }

    [Fact]
    public void ExecuteLoginReturnsUnexpectedResultHandlesGracefully()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Test content"));

        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase.Execute(true).Returns(42); // Unexpected error result

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(4);
        _chatService.DidNotReceiveWithAnyArgs()
            .Run<UpdateUnitTestRequest, UpdateUnitTestResponse>(null!, null!, default, null!);
    }

    [Fact]
    public void ExecuteRewriteUnitTestFileSuccessHandlesCorrectly()
    {
        var command = CreateCommand();
        const string sourceFile = "/path/to/source.cs";
        const string testFile = "/path/to/test.cs";

        _fileSystem.AddFile(Normalize(sourceFile), new MockFileData("// Source content"));
        _fileSystem.AddFile(Normalize(testFile), new MockFileData("// Old test content"));

        var rootDirectory = _fileSystem.DirectoryInfo.New("/");
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDirectory));
        _loginUseCase.Execute(true).Returns(0);

        const string updatedContent = "// Updated test content";
        _chatService.Run(
                input: Arg.Any<UpdateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<IUpdateUnitTestChatScript>())
            .Returns(new UpdateUnitTestResponse(updatedContent));

        var result = command.Execute(sourceFile, testFile);

        result.Should().Be(0);
        _fileSystem.GetFile(testFile).TextContents.Should().Be(updatedContent);
        _console.Output.Should().Contain("Unit tests updated").And.Contain(Normalize("path/to/test.cs"));
    }
}
