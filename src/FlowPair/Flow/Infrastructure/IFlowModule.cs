using Ciandt.FlowTools.FlowPair.Flow.Operations.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.GenerateToken;
using Ciandt.FlowTools.FlowPair.Flow.Operations.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Flow.Infrastructure;

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
