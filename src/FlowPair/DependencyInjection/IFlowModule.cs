using Ciandt.FlowTools.FlowPair.Flow;
using Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.GenerateToken;
using Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProviderModule]
[Singleton(typeof(FlowHttpClient))]
[Singleton(typeof(IFlowAuthService), typeof(FlowAuthService))]
[Singleton(typeof(IProxyClient), typeof(ProxyClient))]
[Singleton(typeof(IOpenAiClient), typeof(OpenAiClient))]
[Singleton(typeof(IAnthropicClient), typeof(AnthropicClient))]
public interface IFlowModule;
