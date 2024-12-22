using Ciandt.FlowTools.FlowPair.Settings.Operations.Configure;
using Ciandt.FlowTools.FlowPair.Settings.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Settings.Infrastructure;

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
