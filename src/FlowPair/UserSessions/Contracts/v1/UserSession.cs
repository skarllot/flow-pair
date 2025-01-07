using Raiqub.LlmTools.FlowPair.Support.Persistence;

namespace Raiqub.LlmTools.FlowPair.UserSessions.Contracts.v1;

public sealed record UserSession(
    Version Version,
    string AccessToken,
    DateTimeOffset ExpiresAt) : IVersionedJsonValue
{
    public static Version CurrentVersion { get; } = new(1, 0);

    public static UserSession Empty() => new(
        Version: CurrentVersion,
        AccessToken: string.Empty,
        ExpiresAt: DateTimeOffset.MinValue);

    public static UserSession New(string accessToken, DateTimeOffset expiresAt) => new(
        Version: CurrentVersion,
        AccessToken: accessToken,
        ExpiresAt: expiresAt);

    public bool IsExpired(TimeProvider timeProvider) => timeProvider.GetUtcNow() >= ExpiresAt;
}
