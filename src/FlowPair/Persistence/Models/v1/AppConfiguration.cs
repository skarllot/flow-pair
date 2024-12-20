using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Persistence.Models.v1;

public sealed record AppConfiguration(
    Version Version,
    string Tenant,
    string ClientId,
    string ClientSecret)
{
    public const string LlmAppCode = "llm-api";

    public ImmutableList<string> Apps { get; init; } = [LlmAppCode];
}
