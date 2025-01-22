using System.Collections.Immutable;
using System.IO.Abstractions;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Flow.Contracts;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Tests.Testing;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.UpdateUnitTest;

[TestSubject(typeof(UpdateUnitTestChatScript))]
public class UpdateUnitTestChatScriptTest
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private readonly IProjectFilesMessageFactory _projectFilesMessageFactory;
    private readonly IDirectoryStructureMessageFactory _directoryStructureMessageFactory;
    private readonly UpdateUnitTestChatScript _chatScript;

    public UpdateUnitTestChatScriptTest()
    {
        _projectFilesMessageFactory = Substitute.For<IProjectFilesMessageFactory>();
        _directoryStructureMessageFactory = Substitute.For<IDirectoryStructureMessageFactory>();
        _chatScript = new UpdateUnitTestChatScript(_projectFilesMessageFactory, _directoryStructureMessageFactory);
    }

    [Theory]
    [JsonResourceData("20250108114920-history.json")]
    public void ConvertResultShouldExtractOutputs(ImmutableList<Message> messages)
    {
        // Arrange
        _projectFilesMessageFactory
            .CreateWithProjectFilesContent(
                Arg.Any<IDirectoryInfo>(),
                Arg.Any<IEnumerable<string>?>(),
                Arg.Any<IEnumerable<string>?>())
            .Returns(new Message(SenderRole.User, "// Projects"));

        _directoryStructureMessageFactory
            .CreateWithRepositoryStructure(Arg.Any<IDirectoryInfo>())
            .Returns(new Message(SenderRole.User, "// Structure"));

        var assistantMessages = messages
            .Where(m => m.Role == SenderRole.Assistant)
            .Select(Ok<Message, FlowError>)
            .ToImmutableList();

        var completeChatHandler = Substitute.For<IProxyCompleteChatHandler>();
        var tempFileWriter = Substitute.For<ITempFileWriter>();

        var chatService = new ChatService(
            timeProvider: _timeProvider,
            jsonContext: ChatJsonContext.Default,
            completeChatHandler: completeChatHandler,
            tempFileWriter: tempFileWriter);

        completeChatHandler
            .ChatCompletion(Arg.Any<LlmModelType>(), Arg.Any<ImmutableList<Message>>())
            .Returns(assistantMessages[0], assistantMessages.Skip(1).ToArray());

        // Act
        var result = chatService.Run(
            input: new UpdateUnitTestRequest(
                SourceFileInfo: Substitute.For<IFileInfo>(),
                TestFileInfo: Substitute.For<IFileInfo>(),
                RootPath: Substitute.For<IDirectoryInfo>()),
            progress: new Progress(new TestConsole()),
            llmModelType: LlmModelType.Gpt4o,
            chatScript: _chatScript);

        // Assert
        result.Should().BeOk().Should()
            .Match((UpdateUnitTestResponse x) => x.Content.Length > 0);
    }
}
