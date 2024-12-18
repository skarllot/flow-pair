using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Flow.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Persistence;

namespace Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat;

public partial interface IProxyClient;

[GenerateAutomaticInterface]
public sealed class ProxyClient(
    IConfigurationService configurationService,
    IUserSessionService userSessionService,
    IFlowAuthService authService,
    IOpenAiClient openAiClient,
    IAnthropicClient anthropicClient)
    : IProxyClient
{
    public Result<Message, FlowError> ChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from configuration in configurationService.CurrentAppConfiguration
                .MapErr(v => new FlowError(0, v))
            from session in userSessionService.UserSession
                .MapErr(v => new FlowError(0, v))
            from token in authService.RequestToken(configuration, session)
            from chat in AnthropicChatCompletion(model, messages)
                .Match(m => m, _ => OpenAiChatCompletion(model, messages))
            select chat;
    }

    private Result<Message, FlowError> AnthropicChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from anthropic in model.ToAnthropic()
                .OkOrElse(() => new FlowError(0, "Model is not an Anthropic."))
            from message in anthropicClient.ChatCompletion(anthropic, messages)
            select message;
    }

    private Result<Message, FlowError> OpenAiChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from openAiModels in model.ToOpenAi()
                .OkOrElse(() => new FlowError(0, "Model is not an Open AI."))
            from message in openAiClient.ChatCompletion(openAiModels, messages)
            select message;
    }
}
