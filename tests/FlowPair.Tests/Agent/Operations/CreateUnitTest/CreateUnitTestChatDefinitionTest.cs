using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Chats.Infrastructure;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Chats.Services;
using Ciandt.FlowTools.FlowPair.Flow.Contracts;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using Ciandt.FlowTools.FlowPair.Tests.Testing;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Operations.CreateUnitTest;

[TestSubject(typeof(CreateUnitTestChatDefinition))]
public class CreateUnitTestChatDefinitionTest
{
    private readonly CreateUnitTestChatDefinition _chatDefinition = new(AgentJsonContext.Default);

    [Theory]
    [JsonResourceData("20250106175550-history.json")]
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
            .Match((CreateUnitTestResponse x) => x.FilePath.Length > 0 && x.Content.Length > 0);
    }
}
