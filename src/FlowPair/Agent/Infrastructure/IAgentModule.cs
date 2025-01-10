using Jab;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(AgentJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IDirectoryStructureMessageFactory), typeof(DirectoryStructureMessageFactory))]
[Singleton(typeof(IProjectFilesMessageFactory), typeof(ProjectFilesMessageFactory))]

// Chat scripts
[Singleton(typeof(IReviewChatScript), typeof(ReviewChatScript))]
[Singleton(typeof(ICreateUnitTestChatDefinition), typeof(CreateUnitTestChatDefinition))]
[Singleton(typeof(IUpdateUnitTestChatDefinition), typeof(UpdateUnitTestChatDefinition))]
[Singleton(typeof(IDistributeReviewChatDefinitionFactory), typeof(DistributeReviewChatDefinitionFactory))]

// Operations
[Singleton(typeof(ILoginUseCase), typeof(LoginUseCase))]
[Singleton(typeof(LoginCommand))]
[Singleton(typeof(ReviewChangesCommand))]
[Singleton(typeof(CreateUnitTestCommand))]
[Singleton(typeof(UpdateUnitTestCommand))]
public interface IAgentModule
{
    static AgentJsonContext GetJsonContext() => AgentJsonContext.Default;
}
