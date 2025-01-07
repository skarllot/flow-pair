using Ciandt.FlowTools.FlowPair.Chats.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.Chats.Infrastructure;

[ServiceProviderModule]

// Infrastructure
[Singleton(typeof(ChatJsonContext), Factory = nameof(GetJsonContext))]

// Services
[Singleton(typeof(IChatService), typeof(ChatService))]
public interface IChatModule
{
    static ChatJsonContext GetJsonContext() => ChatJsonContext.Default;
}
