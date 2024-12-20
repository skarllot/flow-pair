using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat.v1;

public sealed record AnthropicChatCompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("role")] AnthropicRole Role,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("content")] ImmutableList<AnthropicContent> Content,
    [property: JsonPropertyName("usage")] AnthropicCompletionUsage? Usage);

public sealed record AnthropicCompletionUsage(
    [property: JsonPropertyName("input_tokens")] int InputTokens,
    [property: JsonPropertyName("output_tokens")] int OutputTokens);
