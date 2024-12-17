using System.Text.Json.Serialization;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

public sealed record ChatCompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("choices")] List<Choice> Choices,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint,
    [property: JsonPropertyName("object")] ChatCompletionObject ObjectType,
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

//[JsonConverterGenerator(AllowIntegerValues = false)]
[EnumGenerator]
[JsonConverter(typeof(ChatCompletionObjectJsonConverter))]
public enum ChatCompletionObject
{
    [JsonPropertyName("chat.completion")] ChatCompletion
}
