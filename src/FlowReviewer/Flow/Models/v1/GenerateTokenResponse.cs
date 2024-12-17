using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

public sealed record GenerateTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
