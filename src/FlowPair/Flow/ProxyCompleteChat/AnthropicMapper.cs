using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat;

public static class AnthropicMapper
{
    public static Option<AllowedAnthropicModels> ToAnthropic(this AllowedModel allowedModel) => allowedModel.Match(
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

    public static AnthropicRole ToAnthropic(this Role role) => role.Match(
        System: AnthropicRole.Assistant,
        User: AnthropicRole.User,
        Assistant: AnthropicRole.Assistant,
        Function: (AnthropicRole)0);

    public static string? ToAnthropicSystem(this IEnumerable<Message> messages) => messages
        .Where(m => m.Role == Role.System)
        .AggregateToStringLines(m => m.Content);

    public static ImmutableList<AnthropicMessage> ToAnthropicMessages(this ImmutableList<Message> messages) => messages
        .Where(m => m.Role != Role.System)
        .Select(
            m => new AnthropicMessage(
                Role: m.Role.ToAnthropic(),
                Content: [new AnthropicContent(AnthropicMessageType.Text, m.Content)]))
        .ToImmutableList();

    public static Role ToProxy(this AnthropicRole role) => role.Match(
        User: Role.User,
        Assistant: Role.Assistant);

    public static Message ToProxy(this AnthropicMessage message) => new(
        message.Role.ToProxy(),
        message.Content
            .Where(c => c.Type == AnthropicMessageType.Text)
            .AggregateToStringLines(c => c.Text ?? string.Empty)
            .ToString());
}
