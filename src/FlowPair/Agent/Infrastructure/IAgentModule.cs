using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(AgentJsonContext), Factory = nameof(GetJsonContext))]

// Operations
[Singleton(typeof(ILoginUseCase), typeof(LoginUseCase))]
[Singleton(typeof(LoginCommand))]
[Singleton(typeof(ReviewChangesCommand))]
[Singleton(typeof(CreateUnitTestCommand))]
public interface IAgentModule
{
    static AgentJsonContext GetJsonContext() => AgentJsonContext.Default;
}
