namespace Ciandt.FlowTools.FlowReviewer.Persistence;

public sealed record UserSession(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string? PreferredModel = null);
