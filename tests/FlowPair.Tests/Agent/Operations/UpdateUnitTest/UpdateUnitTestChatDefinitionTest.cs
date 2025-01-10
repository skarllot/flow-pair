using System.Collections.Immutable;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
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

[TestSubject(typeof(UpdateUnitTestChatDefinition))]
public class UpdateUnitTestChatDefinitionTest
{
    private readonly UpdateUnitTestChatDefinition _chatDefinition = new();

    [Theory]
    [JsonResourceData("20250108114920-history.json")]
    public void ConvertResultShouldExtractOutputs(ImmutableList<Message> messages)
    {
        // Arrange
        var initialMessages = messages
            .SkipWhile(m => m.Role != SenderRole.User)
            .TakeWhile(m => m.Role == SenderRole.User)
            .SkipLast(1)
            .ToImmutableList();

        var assistantMessages = messages
            .Where(m => m.Role == SenderRole.Assistant)
            .Select(Ok<Message, FlowError>)
            .ToImmutableList();

        var completeChatHandler = Substitute.For<IProxyCompleteChatHandler>();
        var tempFileWriter = Substitute.For<ITempFileWriter>();

        var chatService = new ChatService(
            jsonContext: ChatJsonContext.Default,
            completeChatHandler: completeChatHandler,
            tempFileWriter: tempFileWriter);

        completeChatHandler
            .ChatCompletion(Arg.Any<LlmModelType>(), Arg.Any<ImmutableList<Message>>())
            .Returns(assistantMessages[0], assistantMessages.Skip(1).ToArray());

        // Act
        var result = chatService.Run(
            progress: new Progress(new TestConsole()),
            llmModelType: LlmModelType.Gpt4o,
            chatDefinition: _chatDefinition,
            initialMessages: initialMessages);

        // Assert
        result.Should().BeOk().Should()
            .Match((UpdateUnitTestResponse x) => x.Content.Length > 0);
    }
}
