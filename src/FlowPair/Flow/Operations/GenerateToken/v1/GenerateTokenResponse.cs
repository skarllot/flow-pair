using System.Text.Json.Serialization;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken.v1;

public sealed record GenerateTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
