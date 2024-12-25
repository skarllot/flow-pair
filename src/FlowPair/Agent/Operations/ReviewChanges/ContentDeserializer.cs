using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public static class ContentDeserializer
{
    public static Result<ImmutableList<ReviewerFeedbackResponse>, string> TryDeserializeFeedback(
        ReadOnlySpan<char> content,
        JsonTypeInfo<ImmutableList<ReviewerFeedbackResponse>> typeInfo)
    {
        if (content.IsWhiteSpace())
            return "JSON not found on empty content";

        JsonException? lastException = null;
        while (!content.IsEmpty)
        {
            var start = content.IndexOf('[');
            if (start < 0)
                return "Invalid JSON: '[' not found";

            var end = content.LastIndexOf(']');
            if (end < start)
                return "Invalid JSON: ']' not found or comes before '['";

            try
            {
                return JsonSerializer.Deserialize(content[start..(end + 1)], typeInfo) ?? [];
            }
            catch (JsonException exception)
            {
                lastException = exception;
                content = content[(start + 1)..];
            }
        }

        return lastException?.Message ?? "Invalid JSON";
    }
}
