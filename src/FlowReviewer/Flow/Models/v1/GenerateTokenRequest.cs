namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

public sealed record GenerateTokenRequest(
    string ClientId,
    string ClientSecret,
    string AppToAccess);
