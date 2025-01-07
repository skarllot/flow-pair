using Jab;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Chats.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(ChatJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IChatService), typeof(ChatService))]
public interface IChatModule
{
    static ChatJsonContext GetJsonContext() => ChatJsonContext.Default;
}
