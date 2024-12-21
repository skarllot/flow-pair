using Ciandt.FlowTools.FlowPair.ChangeTracking;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Persistence;
using Ciandt.FlowTools.FlowPair.Persistence.Infrastructure;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProvider]
[Import(typeof(IPersistenceModule))]
[Import(typeof(IFlowModule))]
[Import(typeof(IAgentModule))]
[Import(typeof(IExternalModule))]
[Singleton(typeof(AppJsonContext), Factory = nameof(AppJsonContextDefaultInstance))]
[Singleton(typeof(IUserSessionService), typeof(UserSessionService))]
[Singleton(typeof(IGitDiffExtractor), typeof(GitDiffExtractor))]
public sealed partial class AppContainer
{
    private static AppJsonContext AppJsonContextDefaultInstance() => AppJsonContext.Default;
}
