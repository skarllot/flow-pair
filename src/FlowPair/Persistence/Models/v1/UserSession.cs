using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Persistence.Models.v1;

public sealed record UserSession(
    Version Version,
    string AccessToken,
    DateTimeOffset ExpiresAt,
    ImmutableList<string>? AvailableModels = null,
    string? PreferredModel = null)
{
    public ImmutableList<string> AvailableModels { get; init; } = AvailableModels ?? [];
}
