using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.ChangeTracking;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Infrastructure;
using Ciandt.FlowTools.FlowPair.Settings.Infrastructure;
using Ciandt.FlowTools.FlowPair.UserSessions.Infrastructure;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProvider]
[Import(typeof(ISettingsModule))]
[Import(typeof(IUserSessionModule))]
[Import(typeof(IFlowModule))]
[Import(typeof(IAgentModule))]
[Import(typeof(IExternalModule))]
[Singleton(typeof(AppJsonContext), Factory = nameof(AppJsonContextDefaultInstance))]
[Singleton(typeof(IGitDiffExtractor), typeof(GitDiffExtractor))]
public sealed partial class AppContainer
{
    private static AppJsonContext AppJsonContextDefaultInstance() => AppJsonContext.Default;
}
