namespace Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken.v1;

public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
