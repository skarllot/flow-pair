using Jab;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;

namespace Raiqub.LlmTools.FlowPair.Git.Infrastructure;

[ServiceProviderModule]
[Singleton(typeof(IGitGetChangesHandler), typeof(GitGetChangesHandler))]
public interface IGitModule;
