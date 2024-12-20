using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat.v1;

public sealed record OpenAiMessage(
    [property: JsonPropertyName("role")] OpenAiRole Role,
    [property: JsonPropertyName("content")] string Content);
