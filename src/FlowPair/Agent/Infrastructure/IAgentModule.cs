using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;
using Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(AgentJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IChatService), typeof(ChatService))]
[Singleton(typeof(IDirectoryStructureMessageFactory), typeof(DirectoryStructureMessageFactory))]
[Singleton(typeof(IProjectFilesMessageFactory), typeof(ProjectFilesMessageFactory))]

// Chat definitions
[Singleton(typeof(IReviewChatDefinition), typeof(ReviewChatDefinition))]
[Singleton(typeof(ICreateUnitTestChatDefinition), typeof(CreateUnitTestChatDefinition))]
[Singleton(typeof(IUpdateUnitTestChatDefinition), typeof(UpdateUnitTestChatDefinition))]

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
