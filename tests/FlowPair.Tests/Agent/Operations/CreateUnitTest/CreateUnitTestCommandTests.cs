using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;
using Spectre.Console.Testing;
using static Raiqub.LlmTools.FlowPair.LocalFileSystem.Services.PathAnalyzer;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.CreateUnitTest;

public sealed class CreateUnitTestCommandTests : IDisposable
{
    private readonly TestConsole _console = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly ICreateUnitTestChatScript _chatScript = Substitute.For<ICreateUnitTestChatScript>();
    private readonly IWorkingDirectoryWalker _workingDirectoryWalker = Substitute.For<IWorkingDirectoryWalker>();

    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();

    public void Dispose() => _console.Dispose();

    private CreateUnitTestCommand CreateCommand() => new(
        _console,
        _fileSystem,
        _chatScript,
        _workingDirectoryWalker,
        _loginUseCase,
        _chatService);

    [Fact]
    public void ExecuteFileDoesNotExistReturnsError()
    {
        const string nonExistentFilePath = "/path/to/nonexistent/file.cs";
        var command = CreateCommand();

        var result = command.Execute(nonExistentFilePath);

        result.Should().Be(1);
        _console.Output.Should().Contain("Error").And.Contain("file does not exist");
        _workingDirectoryWalker.DidNotReceiveWithAnyArgs().TryFindRepositoryRoot(Arg.Any<string>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/path/to/nonexistent/example.cs")]
    public void ExecuteExampleFileDoesNotExistReturnsErrorOrContinues(string? exampleFilePath)
    {
        const string filePath = "/path/to/file.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));

        var result = command.Execute(filePath, exampleFilePath);

        if (exampleFilePath != null)
        {
            result.Should().Be(2);
            _console.Output.Should().Contain("Error").And.Contain("example file does not exist");
            _workingDirectoryWalker.DidNotReceiveWithAnyArgs().TryFindRepositoryRoot(Arg.Any<string>());
        }
        else
        {
            _workingDirectoryWalker.Received(1).TryFindRepositoryRoot(Arg.Any<string>());
        }
    }

    [Fact]
    public void ExecuteRepositoryRootNotFoundReturnsError()
    {
        const string filePath = "/path/to/file.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(None);

        var result = command.Execute(filePath);

        result.Should().Be(3);
        _console.Output.Should().Contain("Error").And.Contain("locate Git repository");
        _loginUseCase.DidNotReceiveWithAnyArgs().Execute(Arg.Any<bool>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ExecuteLoginResultReturnsAppropriateResult(int loginResult)
    {
        const string filePath = "/path/to/file.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase
            .Execute(true)
            .Returns(loginResult);

        var result = command.Execute(filePath);

        result.Should().Be(loginResult == 0 ? 5 : 4);
        _loginUseCase.Received(1).Execute(true);
        if (loginResult != 0)
        {
            _chatService.DidNotReceiveWithAnyArgs().Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>());
        }
    }

    [Fact]
    public void ExecuteChatServiceFailsReturnsError()
    {
        const string filePath = "/path/to/file.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase
            .Execute(true)
            .Returns(0);
        _chatService.Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>())
            .Returns("Chat service error");

        var result = command.Execute(filePath);

        result.Should().Be(5);
        _console.Output.Should().Contain("Error").And.Contain("Chat service error");
    }

    [Fact]
    public void ExecuteSuccessCreatesUnitTestFile()
    {
        const string filePath = "/path/to/file.cs";
        const string testFilePath = "/path/to/tests/FileTests.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase
            .Execute(true)
            .Returns(0);

        var response = new CreateUnitTestResponse("path/to/tests/FileTests.cs", "// Generated unit test content");
        _chatService.Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>())
            .Returns(response);

        var result = command.Execute(filePath);

        result.Should().Be(0);
        _fileSystem.File.Exists(Normalize(testFilePath)).Should().BeTrue();
        _fileSystem.File.ReadAllText(Normalize(testFilePath)).Should().Be("// Generated unit test content");
        _console.Output.Should().Contain("File created").And.Contain(response.FilePath);
    }

    [Fact]
    public void ExecuteWithExampleFileIncludesExampleInMessages()
    {
        const string filePath = "/path/to/file.cs";
        const string exampleFilePath = "/path/to/example.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _fileSystem.AddFile(Normalize(exampleFilePath), new MockFileData("// Example test code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase
            .Execute(true)
            .Returns(0);

        var response = new CreateUnitTestResponse("path/to/tests/FileTests.cs", "// Generated unit test content");
        _chatService.Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>())
            .Returns(response);

        var result = command.Execute(filePath, exampleFilePath);

        result.Should().Be(0);
        _chatService.Received(requiredNumberOfCalls: 1).Run(
            input: Arg.Any<CreateUnitTestRequest>(),
            progress: Arg.Any<Progress>(),
            llmModelType: Arg.Any<LlmModelType>(),
            chatScript: Arg.Any<ICreateUnitTestChatScript>());
    }

    [Fact]
    public void ExecuteCreatesDirectoryIfNotExists()
    {
        const string filePath = "/path/to/file.cs";
        const string testFilePath = "/path/to/nonexistent/directory/FileTests.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/")));
        _loginUseCase
            .Execute(true)
            .Returns(0);

        var response = new CreateUnitTestResponse(
            FilePath: "path/to/nonexistent/directory/FileTests.cs",
            Content: "// Generated unit test content");
        _chatService.Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>())
            .Returns(response);

        var result = command.Execute(filePath);

        result.Should().Be(0);
        _fileSystem.Directory.Exists("/path/to/nonexistent/directory").Should().BeTrue();
        _fileSystem.File.Exists(testFilePath).Should().BeTrue();
    }

    [Fact]
    public void ExecuteIncludesProjectFilesAndDirectoryStructure()
    {
        const string filePath = "/path/to/file.cs";
        var command = CreateCommand();

        _fileSystem.AddFile(Normalize(filePath), new MockFileData("// Some code"));
        var rootDirectory = _fileSystem.DirectoryInfo.New("/");
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDirectory));
        _loginUseCase
            .Execute(true)
            .Returns(0);

        var response = new CreateUnitTestResponse("path/to/tests/FileTests.cs", "// Generated unit test content");
        _chatService.Run(
                input: Arg.Any<CreateUnitTestRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: Arg.Any<LlmModelType>(),
                chatScript: Arg.Any<ICreateUnitTestChatScript>())
            .Returns(response);

        var result = command.Execute(filePath);

        result.Should().Be(0);
        _chatService.Received(requiredNumberOfCalls: 1).Run(
            input: Arg.Any<CreateUnitTestRequest>(),
            progress: Arg.Any<Progress>(),
            llmModelType: Arg.Any<LlmModelType>(),
            chatScript: Arg.Any<ICreateUnitTestChatScript>());
    }
}
