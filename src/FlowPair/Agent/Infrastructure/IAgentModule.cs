using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(AgentJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IDirectoryStructureMessageFactory), typeof(DirectoryStructureMessageFactory))]
[Singleton(typeof(IProjectFilesMessageFactory), typeof(ProjectFilesMessageFactory))]

// Chat definitions
[Singleton(typeof(IReviewChatDefinition), typeof(ReviewChatDefinition))]

// Operations
[Singleton(typeof(ILoginUseCase), typeof(LoginUseCase))]
[Singleton(typeof(LoginCommand))]
[Singleton(typeof(ReviewChangesCommand))]
public interface IAgentModule
{
    static AgentJsonContext GetJsonContext() => AgentJsonContext.Default;
}
