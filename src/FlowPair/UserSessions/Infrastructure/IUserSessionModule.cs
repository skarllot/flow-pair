using Jab;
using Raiqub.LlmTools.FlowPair.UserSessions.Services;

namespace Raiqub.LlmTools.FlowPair.UserSessions.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(UserSessionJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IUserSessionRepository), typeof(UserSessionRepository))]
public interface IUserSessionModule
{
    static UserSessionJsonContext GetJsonContext() => UserSessionJsonContext.Default;
}
