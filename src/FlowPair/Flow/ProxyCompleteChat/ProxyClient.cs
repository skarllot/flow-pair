using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.GenerateToken;
using Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Persistence;
using Ciandt.FlowTools.FlowPair.Persistence.Services;

namespace Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat;

public partial interface IProxyClient;

[GenerateAutomaticInterface]
public sealed class ProxyClient(
    IAppSettingsRepository appSettingsRepository,
    IUserSessionService userSessionService,
    IFlowAuthService authService,
    IOpenAiClient openAiClient,
    IAnthropicClient anthropicClient)
    : IProxyClient
{
    public Result<Message, FlowError> ChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from configuration in appSettingsRepository.GetConfiguration()
                .MapErr(v => new FlowError(0, v.ToString()))
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
            from message in anthropicClient
                .ChatCompletion(anthropic, messages.ToAnthropicMessages(), messages.ToAnthropicSystem())
            select message.ToProxy();
    }

    private Result<Message, FlowError> OpenAiChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from openAiModels in model.ToOpenAi()
                .OkOrElse(() => new FlowError(0, "Model is not an Open AI."))
            from message in openAiClient.ChatCompletion(openAiModels, messages.ConvertAll(m => m.ToOpenAi()))
            select message.ToProxy();
    }
}
