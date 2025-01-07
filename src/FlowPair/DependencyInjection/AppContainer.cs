using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Chats.Infrastructure;
using Ciandt.FlowTools.FlowPair.Flow.Infrastructure;
using Ciandt.FlowTools.FlowPair.Git.Infrastructure;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Infrastructure;
using Ciandt.FlowTools.FlowPair.Settings.Infrastructure;
using Ciandt.FlowTools.FlowPair.UserSessions.Infrastructure;
using Jab;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProvider]
[Import(typeof(IAgentModule))]
[Import(typeof(IChatModule))]
[Import(typeof(IFlowModule))]
[Import(typeof(IGitModule))]
[Import(typeof(ILocalFileSystemModule))]
[Import(typeof(ISettingsModule))]
[Import(typeof(IUserSessionModule))]
[Import(typeof(IExternalModule))]
public sealed partial class AppContainer;
