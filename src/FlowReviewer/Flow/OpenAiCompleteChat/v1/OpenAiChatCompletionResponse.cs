using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat.v1;

public sealed record OpenAiChatCompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("choices")] List<Choice> Choices,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint,
    [property: JsonPropertyName("usage")] CompletionUsage? Usage);

public sealed record Choice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("finish_reason")] string FinishReason,
    [property: JsonPropertyName("message")] Message Message);

/// <param name="PromptTokens">Number of tokens in the prompt.</param>
/// <param name="CompletionTokens">Number of tokens in the generated completion.</param>
/// <param name="TotalTokens">Total number of tokens used in the request (prompt + completion).</param>
public sealed record CompletionUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens);
