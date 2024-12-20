using Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat;

public static class OpenAiMapper
{
    public static Option<AllowedOpenAiModels> ToOpenAi(this AllowedModel allowedModel) => allowedModel.Match(
        Gpt4: () => AllowedOpenAiModels.Gpt4,
        Gpt4o: () => AllowedOpenAiModels.Gpt4o,
        Gpt4oMini: () => AllowedOpenAiModels.Gpt4oMini,
        Gpt35Turbo: () => AllowedOpenAiModels.Gpt35Turbo,
        TextEmbeddingAda002: () => AllowedOpenAiModels.TextEmbeddingAda002,
        TextEmbeddingGecko003: () => Option<AllowedOpenAiModels>.None,
        Gemini15Flash: () => Option<AllowedOpenAiModels>.None,
        Gemini15Pro: () => Option<AllowedOpenAiModels>.None,
        Claude3Sonnet: () => Option<AllowedOpenAiModels>.None,
        Claude35Sonnet: () => Option<AllowedOpenAiModels>.None,
        Llama370b: () => Option<AllowedOpenAiModels>.None);

    public static OpenAiRole ToOpenAi(this Role role) => role.Match(
        System: OpenAiRole.System,
        User: OpenAiRole.User,
        Assistant: OpenAiRole.Assistant,
        Function: OpenAiRole.Function);

    public static OpenAiMessage ToOpenAi(this Message message) => new(
        Role: message.Role.ToOpenAi(),
        Content: message.Content);

    public static Role ToProxy(this OpenAiRole role) => role.Match(
        System: Role.System,
        User: Role.User,
        Assistant: Role.Assistant,
        Function: Role.Function);

    public static Message ToProxy(this OpenAiMessage message) => new(
        Role: message.Role.ToProxy(),
        Content: message.Content);
}
