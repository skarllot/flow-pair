using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken.v1;
using Ciandt.FlowTools.FlowReviewer.Persistence;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken;

public partial interface IFlowAuthService;

[GenerateAutomaticInterface]
public sealed class FlowAuthService(
    IAnsiConsole console,
    FlowHttpClient httpClient,
    AppJsonContext jsonContext,
    IUserSessionService userSessionService)
    : IFlowAuthService
{
    public Result<UserSession, FlowError> RequestToken(AppConfiguration configuration, UserSession userSession)
    {
        httpClient.FlowTenant = configuration.Tenant;

        var now = DateTimeOffset.UtcNow;
        if (userSession.ExpiresAt > now)
        {
            httpClient.BearerToken = userSession.AccessToken;
            return userSession;
        }

        console.Write("Generating Flow token...");
        using var responseMessage = httpClient.PostAsJson(
            requestUri: "/auth-engine-api/v1/api-key/token",
            value: new GenerateTokenRequest(
                configuration.ClientId,
                configuration.ClientSecret,
                AppConfiguration.LlmAppCode),
            jsonTypeInfo: jsonContext.GenerateTokenRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            console.WriteLine(" FAIL");
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to get Flow access token",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.GenerateTokenResponse);
        if (response is null)
        {
            console.WriteLine(" FAIL");
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved Flow access token is null");
        }

        userSession = userSession with
        {
            AccessToken = response.AccessToken, ExpiresAt = now + TimeSpan.FromSeconds(response.ExpiresIn)
        };

        httpClient.BearerToken = userSession.AccessToken;

        userSessionService.Save(userSession);
        console.WriteLine(" OK");
        return userSession;
    }
}
