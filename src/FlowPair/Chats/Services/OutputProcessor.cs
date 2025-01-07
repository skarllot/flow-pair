using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Chats.Models;

namespace Raiqub.LlmTools.FlowPair.Chats.Services;

public static class OutputProcessor
{
    public static Option<TResult> GetFirst<TResult>(ChatWorkspace chatWorkspace, string key)
        where TResult : class => chatWorkspace.ChatThreads
        .Select(t => t.Outputs.GetValueOrDefault(key) as TResult)
        .WhereNotNull()
        .FirstOrNone();

    public static ImmutableList<TResult> AggregateLists<TResult>(ChatWorkspace chatWorkspace, string key) =>
        chatWorkspace.ChatThreads
            .Select(t => t.Outputs.GetValueOrDefault(key) as ImmutableList<TResult>)
            .WhereNotNull()
            .Aggregate(ImmutableList<TResult>.Empty, (curr, next) => curr.AddRange(next));
}
