using System.Text;

namespace Ciandt.FlowTools.FlowPair.Common;

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
}
