using Jab;
using Raiqub.LlmTools.FlowPair.Settings.Operations.Configure;
using Raiqub.LlmTools.FlowPair.Settings.Services;

namespace Raiqub.LlmTools.FlowPair.Settings.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(SettingsJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IAppSettingsRepository), typeof(AppSettingsRepository))]

// Operations
[Singleton(typeof(ConfigureCommand))]
public interface ISettingsModule
{
    static SettingsJsonContext GetJsonContext() => SettingsJsonContext.Default;
}
