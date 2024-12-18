using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

/// <param name="Role">The role of the messages author.</param>
/// <param name="Content">The contents of the message. content is required for all messages except assistant messages with function calls.</param>
public sealed record Message(
    [property: JsonPropertyName("role")] Role Role,
    [property: JsonPropertyName("content")] string Content);
