using System.Collections.Immutable;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Flow.Contracts;
using Raiqub.LlmTools.FlowPair.Flow.Operations.AnthropicCompleteChat;
using Raiqub.LlmTools.FlowPair.Flow.Operations.OpenAiCompleteChat;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;

public partial interface IProxyCompleteChatHandler;

[GenerateAutomaticInterface]
public sealed class ProxyCompleteChatHandler(
    IOpenAiCompleteChatHandler openAiHandler,
    IAnthropicCompleteChatHandler anthropicHandler)
    : IProxyCompleteChatHandler
{
    public Result<Message, FlowError> ChatCompletion(LlmModelType llmModelType, ImmutableList<Message> messages)
    {
        return llmModelType switch
        {
            _ when llmModelType.ToAnthropic().IsSome => AnthropicChatCompletion(llmModelType, messages),
            _ when llmModelType.ToOpenAi().IsSome => OpenAiChatCompletion(llmModelType, messages),
            _ => new FlowError(0, "Unsupported LLM model type", llmModelType.ToString())
        };
    }

    private Result<Message, FlowError> AnthropicChatCompletion(LlmModelType llmModelType, ImmutableList<Message> messages)
    {
        return from anthropic in llmModelType.ToAnthropic()
                .OkOrElse(() => new FlowError(0, "Model is not an Anthropic."))
            from message in anthropicHandler
                .ChatCompletion(anthropic, messages.ToAnthropicMessages(), messages.ToAnthropicSystem())
            select message.ToProxy();
    }

    private Result<Message, FlowError> OpenAiChatCompletion(LlmModelType llmModelType, ImmutableList<Message> messages)
    {
        return from openAiModels in llmModelType.ToOpenAi()
                .OkOrElse(() => new FlowError(0, "Model is not an Open AI."))
            from message in openAiHandler.ChatCompletion(openAiModels, messages.ConvertAll(m => m.ToOpenAi()))
            select message.ToProxy();
    }
}
