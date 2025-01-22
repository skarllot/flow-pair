using System.Collections.Immutable;
using System.Text.RegularExpressions;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Flow.Contracts;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Tests.Testing;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.ReviewChanges;

[TestSubject(typeof(ReviewChatScript))]
public class ReviewChatScriptTest
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private readonly ReviewChatScript _chatScript;

    public ReviewChatScriptTest()
    {
        _chatScript = new ReviewChatScript(AgentJsonContext.Default);
    }

    [Theory]
    [JsonResourceData("20250121215555-history.json")]
    public void ConvertResultShouldExtractOutputs(ImmutableList<ImmutableList<Message>> messages)
    {
        // Arrange
        var completeChatHandler = new CompleteChatHandler(messages);
        var tempFileWriter = Substitute.For<ITempFileWriter>();

        var chatService = new ChatService(
            timeProvider: _timeProvider,
            jsonContext: ChatJsonContext.Default,
            completeChatHandler: completeChatHandler,
            tempFileWriter: tempFileWriter);

        // Act
        var result = chatService.Run(
            input: new ReviewChangesRequest("diff"),
            progress: new Progress(new TestConsole()),
            llmModelType: LlmModelType.Gpt4o,
            chatScript: _chatScript);

        // Assert
        result.Should().BeOk().Should().HaveCount(4);
    }

    private sealed class CompleteChatHandler : IProxyCompleteChatHandler
    {
        private readonly ImmutableList<ImmutableList<Message>> _messages;
        private List<IEnumerator<Message>> _enumerators;

        public CompleteChatHandler(ImmutableList<ImmutableList<Message>> messages)
        {
            _messages = messages;

            _enumerators = messages
                .Select(l => l.Where(m => m.Role == SenderRole.Assistant).GetEnumerator())
                .ToList();
        }

        public Result<Message, FlowError> ChatCompletion(LlmModelType llmModelType, ImmutableList<Message> messages)
        {
            var stopKeyword = messages
                .Where(m => Regex.IsMatch(m.Content, "(.+?)reply with \"<.+?>\"(.+)"))
                .Select(m => Regex.Replace(m.Content, ".+?reply with \"<(.+?)>\".+", "$1"))
                .FirstOrDefault();
            var replacedMessages = stopKeyword is not null
                ? messages
                    .Select(m => m with { Content = m.Content.Replace(stopKeyword, "f5fc95a5") })
                    .ToImmutableList()
                : messages;
            var enumerator = _enumerators[GetThreadIndex(replacedMessages)];
            enumerator.MoveNext();
            var current = enumerator.Current;
            return stopKeyword is not null
                ? current with { Content = current.Content.Replace("f5fc95a5", stopKeyword) }
                : current;
        }

        private int GetThreadIndex(ImmutableList<Message> list) => _messages
            .Index()
            .Where(
                t => t.Item.Zip(list).All(
                    x => x.First.Content.ReplaceLineEndings() == x.Second.Content.ReplaceLineEndings() ||
                         (x.First.Content.Contains("Follow below a set of changes for review") &&
                          x.Second.Content.Contains("Follow below a set of changes for review"))))
            .Select(x => x.Index)
            .First();
    }
}
