using Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure;
using Ciandt.FlowTools.FlowPair.Persistence.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Persistence.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(PersistenceJsonContext))]

// Services
[Singleton(typeof(IAppSettingsRepository), typeof(AppSettingsRepository))]

// Operations
[Singleton(typeof(ConfigureCommand))]
public interface IPersistenceModule;
