using System.Net.Http.Headers;
using System.Net.Mime;

namespace Ciandt.FlowTools.FlowReviewer.Flow;

public sealed class FlowHttpClient : HttpClient
{
    private const string FlowBaseAddress = "https://flow.ciandt.com/";

    public FlowHttpClient() : base(new SocketsHttpHandler())
    {
        BaseAddress = new Uri(FlowBaseAddress);
        DefaultRequestHeaders.Add("FlowAgent", "local-flow-reviewer");
        DefaultRequestHeaders.Add("FlowOperationId", Guid.NewGuid().ToString("N"));
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    public string? FlowTenant
    {
        get => DefaultRequestHeaders.TryGetValues("FlowTenant", out var values) ? values.FirstOrDefault() : null;
        set => _ = value is null
            ? DefaultRequestHeaders.Remove("FlowTenant")
            : DefaultRequestHeaders.TryAddWithoutValidation("FlowTenant", value);
    }

    public string? BearerToken
    {
        get => DefaultRequestHeaders.TryGetValues("Authorization", out var values)
            ? values.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
            : null;
        set => _ = value is null
            ? DefaultRequestHeaders.Remove("Authorization")
            : DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {value}");
    }
}
