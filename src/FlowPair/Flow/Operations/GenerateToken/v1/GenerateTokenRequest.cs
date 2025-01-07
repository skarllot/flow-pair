namespace Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken.v1;

/// <remarks>
/// See <a href="https://flow.ciandt.com/auth-engine-api#/api-key/generate-token">/FLOW Auth Engine API</a>.
/// </remarks>
public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
