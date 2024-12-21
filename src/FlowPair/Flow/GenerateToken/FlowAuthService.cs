using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.GenerateToken.v1;
using Ciandt.FlowTools.FlowPair.Flow.Infrastructure;
using Ciandt.FlowTools.FlowPair.Settings.Contracts.v1;
using Ciandt.FlowTools.FlowPair.UserSessions.Contracts.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.GenerateToken;

public partial interface IFlowAuthService;

[GenerateAutomaticInterface]
public sealed class FlowAuthService(
    FlowHttpClient httpClient,
    AppJsonContext jsonContext)
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

        using var responseMessage = httpClient.PostAsJson(
            requestUri: "/auth-engine-api/v1/api-key/token",
            value: new GenerateTokenRequest(
                configuration.ClientId,
                configuration.ClientSecret,
                AppConfiguration.LlmAppCode),
            jsonTypeInfo: jsonContext.GenerateTokenRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to get Flow access token",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.GenerateTokenResponse);
        if (response is null)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved Flow access token is null");
        }

        userSession = userSession with
        {
            AccessToken = response.AccessToken, ExpiresAt = now + TimeSpan.FromSeconds(response.ExpiresIn)
        };

        httpClient.BearerToken = userSession.AccessToken;

        return userSession;
    }
}
