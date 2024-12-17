using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.ChangeTracking;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Persistence;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.Flow;

public partial interface ILlmClient : IDisposable;

[GenerateAutomaticInterface]
public sealed class LlmClient(
    AppJsonContext jsonContext,
    IAnsiConsole console,
    IConfigurationService configurationService,
    IUserSessionService userSessionService)
    : ILlmClient
{
    private const string FlowBaseAddress = "https://flow.ciandt.com/";
    private HttpClient? _client;

    [IgnoreAutomaticInterface]
    public void Dispose() => _client?.Dispose();

    private static HttpClient CreateHttpClient(AppConfiguration configuration)
    {
        return new HttpClient(new SocketsHttpHandler())
        {
            DefaultRequestHeaders =
            {
                { "FlowTenant", configuration.Tenant },
                { "FlowAgent", "LocalFlowReviewer" },
                { "Accept", "application/json" }
            },
            BaseAddress = new Uri(FlowBaseAddress)
        };
    }

    public Option<string> Prompt(ImmutableList<FileChange> changes)
    {
        var configuration = configurationService.CurrentAppConfiguration.Unwrap();
        LoadToken(configuration);
        return Some("");
    }

    private void LoadToken(AppConfiguration configuration)
    {
        var now = DateTimeOffset.UtcNow;
        var userSession = userSessionService.UserSession.Unwrap();
        if (userSession.ExpiresAt > now)
        {
            return;
        }

        _client ??= CreateHttpClient(configuration);

        var responseMessage = _client.PostAsJson(
            requestUri: "/auth-engine-api/v1/api-key/token",
            value: new GenerateTokenRequest(
                configuration.ClientId,
                configuration.ClientSecret,
                AppConfiguration.LlmAppCode),
            jsonTypeInfo: jsonContext.GenerateTokenRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            console.MarkupLine($"[bold]Failed to get access token:[/] {responseMessage.Content.ReadAsString()}");
            return;
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.GenerateTokenResponse);
        if (response is null)
        {
            return;
        }

        userSessionService.Save(
            userSession with
            {
                AccessToken = response.AccessToken, ExpiresAt = now + TimeSpan.FromSeconds(response.ExpiresIn)
            });
    }
}
