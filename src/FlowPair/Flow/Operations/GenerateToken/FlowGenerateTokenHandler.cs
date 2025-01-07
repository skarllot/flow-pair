using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Flow.Contracts;
using Raiqub.LlmTools.FlowPair.Flow.Infrastructure;
using Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken.v1;
using Raiqub.LlmTools.FlowPair.Settings.Contracts.v1;
using Raiqub.LlmTools.FlowPair.UserSessions.Contracts.v1;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken;

public partial interface IFlowGenerateTokenHandler;

[GenerateAutomaticInterface]
public sealed class FlowGenerateTokenHandler(
    FlowHttpClient httpClient,
    FlowJsonContext jsonContext)
    : IFlowGenerateTokenHandler
{
    public Result<UserSession, FlowError> Execute(AppConfiguration configuration, UserSession userSession)
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
