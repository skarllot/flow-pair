using System.Text;

namespace Raiqub.LlmTools.FlowPair.Common;

public static class CollectionExtensions
{
    public static string AggregateToStringLines<T>(this IEnumerable<T> collection, Func<T, string> selector)
    {
        return collection
            .Aggregate(
                new StringBuilder(),
                (curr, item) =>
                    curr.Length > 0
                        ? curr.AppendLine().AppendLine().Append(selector(item))
                        : curr.Append(selector(item)))
            .ToString();
    }

    public static Result<TSource, SingleElementProblem> TrySingle<TSource>(this IEnumerable<TSource> source)
        where TSource : notnull
    {
        if (source is IList<TSource> list)
        {
            switch (list.Count)
            {
                case 0:
                    return SingleElementProblem.Empty;
                case 1:
                    return list[0];
            }
        }
        else
        {
            using var e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                return SingleElementProblem.Empty;
            }

            var result = e.Current;
            if (!e.MoveNext())
            {
                return result;
            }
        }

        return SingleElementProblem.MoreThanOneElement;
    }
}
