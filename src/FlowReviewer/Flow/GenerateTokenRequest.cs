namespace Ciandt.FlowTools.FlowReviewer.Flow;

public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
