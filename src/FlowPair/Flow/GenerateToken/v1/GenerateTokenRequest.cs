namespace Ciandt.FlowTools.FlowPair.Flow.GenerateToken.v1;

public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
