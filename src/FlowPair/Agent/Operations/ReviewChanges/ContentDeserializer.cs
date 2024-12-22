using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public static class ContentDeserializer
{
    public static ImmutableList<ReviewerFeedbackResponse> TryDeserializeFeedback(
        ReadOnlySpan<char> content,
        JsonTypeInfo<ImmutableList<ReviewerFeedbackResponse>> typeInfo)
    {
        if (content.IsWhiteSpace())
            return [];

        while (!content.IsEmpty)
        {
            var start = content.IndexOf('[');
            if (start < 0)
                return [];

            var end = content.IndexOf(']');
            if (end < start)
                return [];

            try
            {
                return JsonSerializer.Deserialize(content[start..(end + 1)], typeInfo) ?? [];
            }
            catch (JsonException)
            {
                content = content[(end + 1)..];
            }
        }

        return [];
    }
}
