namespace Ciandt.FlowTools.FlowPair.Flow.Operations.GenerateToken.v1;

public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
