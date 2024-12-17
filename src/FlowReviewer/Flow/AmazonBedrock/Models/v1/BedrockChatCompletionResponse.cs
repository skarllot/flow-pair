using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.AmazonBedrock.Models.v1;

public sealed record BedrockChatCompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("role")] Role Role,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("content")] List<BedrockContent> Content,
    [property: JsonPropertyName("usage")] BedrockCompletionUsage? Usage);

public sealed record BedrockContent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text);

public sealed record BedrockCompletionUsage(
    [property: JsonPropertyName("input_tokens")] int InputTokens,
    [property: JsonPropertyName("output_tokens")] int OutputTokens);
