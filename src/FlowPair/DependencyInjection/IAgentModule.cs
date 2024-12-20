using Ciandt.FlowTools.FlowPair.Agent.ReviewChanges;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProviderModule]
[Singleton(typeof(IFlowChangesReviewer), typeof(FlowChangesReviewer))]
public interface IAgentModule;
