using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;

public static class AnthropicMapper
{
    public static Option<AllowedAnthropicModels> ToAnthropic(this LlmModelType llmModelType) => llmModelType.Match(
        Gpt4: () => Option<AllowedAnthropicModels>.None,
        Gpt4o: () => Option<AllowedAnthropicModels>.None,
        Gpt4oMini: () => Option<AllowedAnthropicModels>.None,
        Gpt35Turbo: () => Option<AllowedAnthropicModels>.None,
        TextEmbeddingAda002: () => Option<AllowedAnthropicModels>.None,
        TextEmbeddingGecko003: () => Option<AllowedAnthropicModels>.None,
        Gemini15Flash: () => Option<AllowedAnthropicModels>.None,
        Gemini15Pro: () => Option<AllowedAnthropicModels>.None,
        Claude3Sonnet: () => AllowedAnthropicModels.Claude3Sonnet,
        Claude35Sonnet: () => AllowedAnthropicModels.Claude35Sonnet,
        Llama370b: () => Option<AllowedAnthropicModels>.None);

    public static AnthropicRole ToAnthropic(this SenderRole role) => role.Match(
        System: AnthropicRole.Assistant,
        User: AnthropicRole.User,
        Assistant: AnthropicRole.Assistant,
        Function: (AnthropicRole)0);

    public static string ToAnthropicSystem(this IEnumerable<Message> messages) => messages
        .Where(m => m.Role == SenderRole.System)
        .AggregateToStringLines(m => m.Content);

    public static ImmutableList<AnthropicMessage> ToAnthropicMessages(this ImmutableList<Message> messages) => messages
        .Where(m => m.Role != SenderRole.System)
        .Select(
            m => new AnthropicMessage(
                Role: m.Role.ToAnthropic(),
                Content: [new AnthropicContent(AnthropicMessageType.Text, m.Content)]))
        .ToImmutableList();

    public static SenderRole ToProxy(this AnthropicRole role) => role.Match(
        User: SenderRole.User,
        Assistant: SenderRole.Assistant);

    public static Message ToProxy(this AnthropicMessage message) => new(
        message.Role.ToProxy(),
        message.Content
            .Where(c => c.Type == AnthropicMessageType.Text)
            .AggregateToStringLines(c => c.Text ?? string.Empty)
            .ToString());
}
