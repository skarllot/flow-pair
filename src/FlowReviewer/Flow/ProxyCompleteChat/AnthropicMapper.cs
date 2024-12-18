using Ciandt.FlowTools.FlowReviewer.Flow.AnthropicCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat;

public static class AnthropicMapper
{
    public static Option<AllowedAnthropicModels> ToAnthropic(this AllowedModel allowedModel)
    {
        return allowedModel.Match(
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
    }
}
