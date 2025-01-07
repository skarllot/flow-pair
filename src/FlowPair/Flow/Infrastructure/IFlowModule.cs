using Jab;
using Raiqub.LlmTools.FlowPair.Flow.Operations.AnthropicCompleteChat;
using Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken;
using Raiqub.LlmTools.FlowPair.Flow.Operations.OpenAiCompleteChat;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;

namespace Raiqub.LlmTools.FlowPair.Flow.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(FlowHttpClient))]
[Singleton(typeof(FlowJsonContext), Factory = nameof(GetJsonContext))]

// Operations
[Singleton(typeof(IFlowGenerateTokenHandler), typeof(FlowGenerateTokenHandler))]
[Singleton(typeof(IProxyCompleteChatHandler), typeof(ProxyCompleteChatHandler))]
[Singleton(typeof(IOpenAiCompleteChatHandler), typeof(OpenAiCompleteChatHandler))]
[Singleton(typeof(IAnthropicCompleteChatHandler), typeof(AnthropicCompleteChatHandler))]
public interface IFlowModule
{
    static FlowJsonContext GetJsonContext() => FlowJsonContext.Default;
}
