using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.ReviewChanges;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[ServiceProviderModule]

// Operations
[Singleton(typeof(ILoginUseCase), typeof(LoginUseCase))]
[Singleton(typeof(LoginCommand))]
[Singleton(typeof(IFlowChangesReviewer), typeof(FlowChangesReviewer))]
public interface IAgentModule;
