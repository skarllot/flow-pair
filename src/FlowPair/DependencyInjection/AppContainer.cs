using Jab;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Flow.Infrastructure;
using Raiqub.LlmTools.FlowPair.Git.Infrastructure;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Infrastructure;
using Raiqub.LlmTools.FlowPair.Settings.Infrastructure;
using Raiqub.LlmTools.FlowPair.UserSessions.Infrastructure;

namespace Raiqub.LlmTools.FlowPair.DependencyInjection;

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
