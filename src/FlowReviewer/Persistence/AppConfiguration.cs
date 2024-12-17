using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowReviewer.Persistence;

public sealed record AppConfiguration(
    string Tenant,
    string ClientId,
    string ClientSecret)
{
    public const string LlmAppCode = "llm-api";

    public ImmutableList<string> Apps { get; init; } = [LlmAppCode];
}
