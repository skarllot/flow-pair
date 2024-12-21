using Ciandt.FlowTools.FlowPair.UserSessions.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.UserSessions.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(UserSessionJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IUserSessionRepository), typeof(UserSessionRepository))]
public interface IUserSessionModule
{
    static UserSessionJsonContext GetJsonContext() => UserSessionJsonContext.Default;
}
