using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public static class OutputProcessor
{
    public static Option<TResult> GetFirst<TResult>(ChatWorkspace chatWorkspace, string key)
        where TResult : class => chatWorkspace.ChatThreads
        .Select(t => CollectionExtensions.GetValueOrDefault(t.Outputs, key) as TResult)
        .WhereNotNull()
        .FirstOrNone();

    public static ImmutableList<TResult> AggregateLists<TResult>(ChatWorkspace chatWorkspace, string key) =>
        chatWorkspace.ChatThreads
            .Select(t => t.Outputs.GetValueOrDefault(key) as ImmutableList<TResult>)
            .WhereNotNull()
            .Aggregate(ImmutableList<TResult>.Empty, (curr, next) => curr.AddRange(next));
}
