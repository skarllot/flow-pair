using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;

public interface IChatDefinition<TResult> : IMessageParser
    where TResult : notnull
{
    ChatScript ChatScript { get; }

    Option<TResult> ConvertResult(ChatWorkspace chatWorkspace);
}
