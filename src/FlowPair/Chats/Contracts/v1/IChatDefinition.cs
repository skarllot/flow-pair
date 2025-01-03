using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Chats.Services;

namespace Ciandt.FlowTools.FlowPair.Chats.Contracts.v1;

public interface IChatDefinition<TResult> : IMessageParser
    where TResult : notnull
{
    ChatScript ChatScript { get; }

    Option<TResult> ConvertResult(ChatWorkspace chatWorkspace);
}
