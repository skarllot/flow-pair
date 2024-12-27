using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Flow.Infrastructure;
using Ciandt.FlowTools.FlowPair.Git.Infrastructure;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Infrastructure;
using Ciandt.FlowTools.FlowPair.Settings.Infrastructure;
using Ciandt.FlowTools.FlowPair.UserSessions.Infrastructure;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProvider]
[Import(typeof(ISettingsModule))]
[Import(typeof(IUserSessionModule))]
[Import(typeof(IGitModule))]
[Import(typeof(IFlowModule))]
[Import(typeof(IAgentModule))]
[Import(typeof(IExternalModule))]
[Import(typeof(ILocalFileSystemModule))]
public sealed partial class AppContainer;
