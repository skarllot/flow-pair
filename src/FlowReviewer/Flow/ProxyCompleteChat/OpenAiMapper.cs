using Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat;

public static class OpenAiMapper
{
    public static Option<AllowedOpenAiModels> ToOpenAi(this AllowedModel allowedModel)
    {
        return allowedModel.Match(
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
    }
}
