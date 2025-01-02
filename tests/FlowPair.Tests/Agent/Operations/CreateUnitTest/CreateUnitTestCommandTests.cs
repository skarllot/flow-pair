using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Serialization.Metadata;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using FluentAssertions;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Operations.CreateUnitTest;

public sealed class CreateUnitTestCommandTests : IDisposable
{
    private readonly TestConsole _console;
    private readonly MockFileSystem _fileSystem;
    private readonly IWorkingDirectoryWalker _workingDirectoryWalker;
    private readonly IProjectFilesMessageFactory _projectFilesMessageFactory;
    private readonly IDirectoryStructureMessageFactory _directoryStructureMessageFactory;
    private readonly ILoginUseCase _loginUseCase;
    private readonly IChatService _chatService;
    private readonly CreateUnitTestCommand _command;

    public CreateUnitTestCommandTests()
    {
        _console = new TestConsole();
        _fileSystem = new MockFileSystem();
        _workingDirectoryWalker = Substitute.For<IWorkingDirectoryWalker>();
        _projectFilesMessageFactory = Substitute.For<IProjectFilesMessageFactory>();
        _directoryStructureMessageFactory = Substitute.For<IDirectoryStructureMessageFactory>();
        _loginUseCase = Substitute.For<ILoginUseCase>();
        _chatService = Substitute.For<IChatService>();

        _command = new CreateUnitTestCommand(
            console: _console,
            fileSystem: _fileSystem,
            jsonContext: AgentJsonContext.Default,
            workingDirectoryWalker: _workingDirectoryWalker,
            projectFilesMessageFactory: _projectFilesMessageFactory,
            directoryStructureMessageFactory: _directoryStructureMessageFactory,
            loginUseCase: _loginUseCase,
            chatService: _chatService
        );

        _projectFilesMessageFactory
            .CreateWithProjectFilesContent(
                Arg.Any<IDirectoryInfo>(),
                Arg.Any<IEnumerable<string>?>(),
                Arg.Any<IEnumerable<string>?>())
            .Returns(new Message(Role.User, "Project files"));
        _directoryStructureMessageFactory
            .CreateWithRepositoryStructure(Arg.Any<IDirectoryInfo>())
            .Returns(new Message(Role.User, "Directory structure"));
    }

    public void Dispose() => _console.Dispose();

    [Fact]
    public void ExecuteFileDoesNotExistReturnsError()
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/path/to/nonexistent/file.cs");

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(1);
        _console.Output.Should().Contain("Error").And.Contain("file does not exist");
        _workingDirectoryWalker.DidNotReceiveWithAnyArgs().TryFindRepositoryRoot(null);
        _loginUseCase.DidNotReceiveWithAnyArgs().Execute(false);
        _chatService.DidNotReceiveWithAnyArgs().RunSingle<CreateUnitTestResponse>(null, default, null, null, null);
    }

    [Fact]
    public void ExecuteRepositoryRootNotFoundReturnsError()
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/path/to/existing/file.cs");
        _fileSystem.AddFile(filePath, new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(None);

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(3);
        _console.Output.Should().Contain("Error").And.Contain("locate Git repository");
        _workingDirectoryWalker.Received(1)
            .TryFindRepositoryRoot(Arg.Is<string>(s => s.Contains(_fileSystem.Path.GetDirectoryName(filePath)!)));
        _loginUseCase.DidNotReceiveWithAnyArgs().Execute(false);
        _chatService.DidNotReceiveWithAnyArgs().RunSingle<CreateUnitTestResponse>(null, default, null, null, null);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void ExecuteLoginFailsReturnsError(int loginErrorCode)
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/path/to/existing/file.cs");
        _fileSystem.AddFile(filePath, new MockFileData("// Some code"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(_fileSystem.DirectoryInfo.New("/repo/root")));

        _loginUseCase.Execute(true).Returns(loginErrorCode);

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(4);
        _loginUseCase.Received(1).Execute(true);
        _chatService.DidNotReceiveWithAnyArgs().RunSingle<CreateUnitTestResponse>(null, default, null, null, null);
    }

    [Fact]
    public void ExecuteChatServiceFailsReturnsError()
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/repo/root/src/file.cs");
        _fileSystem.AddFile(filePath, new MockFileData("// Some code"));
        var rootDir = _fileSystem.DirectoryInfo.New("/repo/root");
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDir));

        _loginUseCase.Execute(true).Returns(0);

        _chatService
            .RunSingle(
                Arg.Any<Progress>(),
                Arg.Any<AllowedModel>(),
                Arg.Any<ChatScript>(),
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>())
            .Returns("Chat service error");

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(5);
        _console.Output.Should().Contain("Error").And.Contain("Chat service error");
        _chatService.Received(1).RunSingle(
            Arg.Any<Progress>(),
            Arg.Any<AllowedModel>(),
            Arg.Any<ChatScript>(),
            Arg.Any<IEnumerable<Message>>(),
            Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>());
    }

    [Fact]
    public void ExecuteSuccessCreatesUnitTestFile()
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/repo/root/src/file.cs");
        const string fileContent = "// Some code";
        _fileSystem.AddFile(filePath, new MockFileData(fileContent));
        var rootDir = _fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize("/repo/root"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDir));

        _loginUseCase.Execute(true).Returns(0);

        var testResponse = new CreateUnitTestResponse(
            FilePath: "tests/FileTests.cs",
            Content: "// Generated unit test");
        _chatService.RunSingle(
                Arg.Any<Progress>(),
                Arg.Any<AllowedModel>(),
                Arg.Any<ChatScript>(),
                Arg.Is<IEnumerable<Message>>(x => x.Count() == 3),
                Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>())
            .Returns(testResponse);

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(0);
        _fileSystem.File.Exists("/repo/root/tests/FileTests.cs").Should().BeTrue();
        _fileSystem.File.ReadAllText("/repo/root/tests/FileTests.cs").Should().Be("// Generated unit test");
        _console.Output.Should().Contain("File created").And
            .Contain(PathAnalyzer.Normalize("/repo/root/tests/FileTests.cs"));

        // Verify all method calls
        _workingDirectoryWalker.Received(1)
            .TryFindRepositoryRoot(Arg.Is<string>(s => s.Contains(_fileSystem.Path.GetDirectoryName(filePath)!)));
        _loginUseCase.Received(1).Execute(true);
        _projectFilesMessageFactory.Received(1).CreateWithProjectFilesContent(rootDir);
        _directoryStructureMessageFactory.Received(1).CreateWithRepositoryStructure(rootDir);
        _chatService.Received(1).RunSingle(
            Arg.Any<Progress>(),
            Arg.Is<AllowedModel>(m => m == AllowedModel.Claude35Sonnet),
            Arg.Is<ChatScript>(m => m == UnitTestChatScript.Default[0]),
            Arg.Is<IEnumerable<Message>>(
                messages =>
                    messages.Any(
                        m => m.Role == Role.User &&
                             m.Content.Contains(PathAnalyzer.Normalize("src/file.cs")) &&
                             m.Content.Contains(fileContent))),
            Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>());
    }

    [Fact]
    public void ExecuteCreatesDirectoryIfNotExists()
    {
        // Arrange
        var filePath = PathAnalyzer.Normalize("/repo/root/src/file.cs");
        _fileSystem.AddFile(filePath, new MockFileData("// Some code"));
        var rootDir = _fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize("/repo/root"));
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDir));

        _loginUseCase.Execute(true).Returns(0);

        var testResponse = new CreateUnitTestResponse(
            FilePath: "tests/nested/directory/FileTests.cs",
            Content: "// Generated unit test");
        _chatService.RunSingle(
                Arg.Any<Progress>(),
                Arg.Any<AllowedModel>(),
                Arg.Any<ChatScript>(),
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>())
            .Returns(testResponse);

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(0);
        _fileSystem.Directory.Exists("/repo/root/tests/nested/directory").Should().BeTrue();
        _fileSystem.File.Exists("/repo/root/tests/nested/directory/FileTests.cs").Should().BeTrue();
        _fileSystem.File.ReadAllText("/repo/root/tests/nested/directory/FileTests.cs").Should()
            .Be("// Generated unit test");
    }

    [Fact]
    public void ExecuteHandlesRelativePaths()
    {
        // Arrange
        const string filePath = "src/file.cs";
        var absolutePath = _fileSystem.Path.GetFullPath(filePath);
        _fileSystem.AddFile(absolutePath, new MockFileData("// Some code"));
        var rootDir = _fileSystem.DirectoryInfo.New(_fileSystem.Path.GetDirectoryName(absolutePath)!);
        _workingDirectoryWalker
            .TryFindRepositoryRoot(Arg.Any<string>())
            .Returns(Some(rootDir));
        _loginUseCase.Execute(true).Returns(0);

        var testResponse = new CreateUnitTestResponse(
            FilePath: "tests/FileTests.cs",
            Content: "// Generated unit test");
        _chatService.RunSingle(
                Arg.Any<Progress>(),
                Arg.Any<AllowedModel>(),
                Arg.Any<ChatScript>(),
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<JsonTypeInfo<CreateUnitTestResponse>>())
            .Returns(testResponse);

        // Act
        var result = _command.Execute(filePath);

        // Assert
        result.Should().Be(0);
        _fileSystem.File.Exists(_fileSystem.Path.Combine(rootDir.FullName, "tests/FileTests.cs")).Should().BeTrue();
    }
}
