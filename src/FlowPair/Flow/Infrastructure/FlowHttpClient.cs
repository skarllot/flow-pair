using System.Net.Http.Headers;
using System.Net.Mime;

namespace Ciandt.FlowTools.FlowPair.Flow.Infrastructure;

public sealed class FlowHttpClient : HttpClient
{
    private const string FlowBaseAddress = "https://flow.ciandt.com/";

    public FlowHttpClient() : base(new SocketsHttpHandler())
    {
        BaseAddress = new Uri(FlowBaseAddress);
        DefaultRequestHeaders.Add("FlowAgent", "flow-pair");
        DefaultRequestHeaders.Add("FlowOperationId", Guid.NewGuid().ToString("N"));
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    public string? FlowTenant
    {
        get => DefaultRequestHeaders.TryGetValues("FlowTenant", out var values) ? values.FirstOrDefault() : null;
        set => SetOrRemoveHeaderValue(DefaultRequestHeaders, "FlowTenant", value);
    }

    public string? BearerToken
    {
        get => DefaultRequestHeaders.TryGetValues("Authorization", out var values)
            ? values.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
            : null;
        set => SetOrRemoveHeaderValue(DefaultRequestHeaders, "Authorization", $"Bearer {value}");
    }

    private static void SetOrRemoveHeaderValue(HttpRequestHeaders headers, string name, string? value)
    {
        headers.Remove(name);

        if (value is not null)
            headers.Add(name, value);
    }
}
