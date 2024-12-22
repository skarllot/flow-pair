using Ciandt.FlowTools.FlowPair.Git.GetChanges;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Git.Infrastructure;

[ServiceProviderModule]
[Singleton(typeof(IGitGetChangesHandler), typeof(GitGetChangesHandler))]
public interface IGitModule;
