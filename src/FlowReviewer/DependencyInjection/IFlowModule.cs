using Ciandt.FlowTools.FlowReviewer.Flow;
using Ciandt.FlowTools.FlowReviewer.Flow.AnthropicCompleteChat;
using Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat;
using Jab;

namespace Ciandt.FlowTools.FlowReviewer.DependencyInjection;

[ServiceProviderModule]
[Singleton(typeof(FlowHttpClient))]
[Singleton(typeof(IFlowAuthService), typeof(FlowAuthService))]
[Singleton(typeof(IProxyClient), typeof(ProxyClient))]
[Singleton(typeof(IOpenAiClient), typeof(OpenAiClient))]
[Singleton(typeof(IAnthropicClient), typeof(AnthropicClient))]
public interface IFlowModule;
