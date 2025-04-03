using Jab;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.Git.Services;

namespace Raiqub.LlmTools.FlowPair.Git.Infrastructure;

[ServiceProviderModule]
[Singleton(typeof(IGitGetChangesHandler), typeof(GitGetChangesHandler))]
[Singleton(typeof(IGitRepositoryFactory), typeof(GitRepositoryFactory))]
public interface IGitModule;
