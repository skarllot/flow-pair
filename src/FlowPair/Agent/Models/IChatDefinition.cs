namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public interface IChatDefinition<TResult> : IOutputParser
    where TResult : notnull
{
    ChatScript ChatScript { get; }

    Option<TResult> ConvertResult(ChatWorkspace chatWorkspace);
}
