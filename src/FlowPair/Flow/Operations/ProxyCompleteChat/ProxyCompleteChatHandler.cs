using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Flow.Contracts;
using Ciandt.FlowTools.FlowPair.Flow.Operations.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;

public partial interface IProxyCompleteChatHandler;

[GenerateAutomaticInterface]
public sealed class ProxyCompleteChatHandler(
    IOpenAiCompleteChatHandler openAiHandler,
    IAnthropicCompleteChatHandler anthropicHandler)
    : IProxyCompleteChatHandler
{
    public Result<Message, FlowError> ChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return AnthropicChatCompletion(model, messages)
            .Match(m => m, _ => OpenAiChatCompletion(model, messages));
    }

    private Result<Message, FlowError> AnthropicChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from anthropic in model.ToAnthropic()
                .OkOrElse(() => new FlowError(0, "Model is not an Anthropic."))
            from message in anthropicHandler
                .ChatCompletion(anthropic, messages.ToAnthropicMessages(), messages.ToAnthropicSystem())
            select message.ToProxy();
    }

    private Result<Message, FlowError> OpenAiChatCompletion(AllowedModel model, ImmutableList<Message> messages)
    {
        return from openAiModels in model.ToOpenAi()
                .OkOrElse(() => new FlowError(0, "Model is not an Open AI."))
            from message in openAiHandler.ChatCompletion(openAiModels, messages.ConvertAll(m => m.ToOpenAi()))
            select message.ToProxy();
    }
}
