using System.Collections.Immutable;
using System.Net.Http.Headers;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.AmazonBedrock;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAi;
using Ciandt.FlowTools.FlowReviewer.Persistence;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow;

public partial interface ILlmClient : IDisposable;

[GenerateAutomaticInterface]
public sealed class LlmClient(
    AppJsonContext jsonContext,
    IConfigurationService configurationService,
    IUserSessionService userSessionService,
    IOpenAiClient openAiClient,
    IBedrockClient bedrockClient)
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

    public Result<Message, FlowError> ChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        IModelClient modelClient = model switch
        {
            AllowedModel.Gpt4 => openAiClient,
            AllowedModel.Gpt4o => openAiClient,
            AllowedModel.Gpt4oMini => openAiClient,
            AllowedModel.Gpt35Turbo => openAiClient,
            AllowedModel.Claude3Sonnet => bedrockClient,
            AllowedModel.Claude35Sonnet => bedrockClient,
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };

        return from configuration in configurationService.CurrentAppConfiguration
                .MapErr(v => new FlowError(0, v))
            from session in userSessionService.UserSession
                .MapErr(v => new FlowError(0, v))
            from token in RequestToken(configuration, session)
            from chat in modelClient.ChatCompletion(_client!, model, messages)
            select chat;
    }

    public Result<ImmutableList<string>, FlowError> GetAvailableModels()
    {
        return from configuration in configurationService.CurrentAppConfiguration
                .MapErr(v => new FlowError(0, v))
            from session in userSessionService.UserSession
                .MapErr(v => new FlowError(0, v))
            from token in RequestToken(configuration, session)
            from models in RequestModels(configuration, session)
            select models.AvailableModels;
    }

    private Result<UserSession, FlowError> RequestToken(AppConfiguration configuration, UserSession userSession)
    {
        _client ??= CreateHttpClient(configuration);

        var now = DateTimeOffset.UtcNow;
        if (userSession.ExpiresAt > now)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userSession.AccessToken);
            return userSession;
        }


        using var responseMessage = _client.PostAsJson(
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
                "Failed to get access token",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.GenerateTokenResponse);
        if (response is null)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved access token is null");
        }

        userSession = userSession with
        {
            AccessToken = response.AccessToken, ExpiresAt = now + TimeSpan.FromSeconds(response.ExpiresIn)
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userSession.AccessToken);

        userSessionService.Save(userSession);
        return userSession;
    }

    private Result<UserSession, FlowError> RequestModels(AppConfiguration configuration, UserSession userSession)
    {
        _client ??= CreateHttpClient(configuration);
        using var responseMessage = _client.Get("/ai-orchestration-api/v1/models");

        if (!responseMessage.IsSuccessStatusCode)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to get available models",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.ImmutableListGetAvailableModelsResponse);
        if (response is null)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved available model list is null");
        }

        userSession = userSession with { AvailableModels = response.Select(x => x.Id).ToImmutableList() };
        userSessionService.Save(userSession);
        return userSession;
    }
}
