using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure.v1;

public sealed record AppConfiguration(
    Version Version,
    string Tenant,
    string ClientId,
    string ClientSecret)
{
    public const string LlmAppCode = "llm-api";
    public static Version CurrentVersion { get; } = new(1, 0);

    public ImmutableList<string> Apps { get; init; } = [LlmAppCode];

    public static AppConfiguration New(string tenant, string clientId, string clientSecret) => new(
        Version: CurrentVersion,
        Tenant: tenant,
        ClientId: clientId,
        ClientSecret: clientSecret);
}
