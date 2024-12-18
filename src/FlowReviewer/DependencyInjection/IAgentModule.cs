using Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges;
using Jab;

namespace Ciandt.FlowTools.FlowReviewer.DependencyInjection;

[ServiceProviderModule]
[Singleton(typeof(IFlowChangesReviewer), typeof(FlowChangesReviewer))]
public interface IAgentModule;
