using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Flow.Operations.OpenAiCompleteChat.v1;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;

public static class OpenAiMapper
{
    public static Option<AllowedOpenAiModels> ToOpenAi(this LlmModelType llmModelType) => llmModelType.Match(
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

    public static OpenAiRole ToOpenAi(this SenderRole role) => role.Match(
        System: OpenAiRole.System,
        User: OpenAiRole.User,
        Assistant: OpenAiRole.Assistant,
        Function: OpenAiRole.Function);

    public static OpenAiMessage ToOpenAi(this Message message) => new(
        Role: message.Role.ToOpenAi(),
        Content: message.Content);

    public static SenderRole ToProxy(this OpenAiRole role) => role.Match(
        System: SenderRole.System,
        User: SenderRole.User,
        Assistant: SenderRole.Assistant,
        Function: SenderRole.Function);

    public static Message ToProxy(this OpenAiMessage message) => new(
        Role: message.Role.ToProxy(),
        Content: message.Content);
}
