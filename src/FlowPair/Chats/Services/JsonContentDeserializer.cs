using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Raiqub.LlmTools.FlowPair.Chats.Services;

public static class JsonContentDeserializer
{
    public static Result<T, string> TryDeserialize<T>(
        ReadOnlySpan<char> content,
        JsonTypeInfo<T> typeInfo)
        where T : notnull
    {
        if (content.IsWhiteSpace())
        {
            return "JSON not found on empty content";
        }

        var (startChar, endChar) = typeInfo.Kind switch
        {
            JsonTypeInfoKind.None => (' ', ' '),
            JsonTypeInfoKind.Object => ('{', '}'),
            JsonTypeInfoKind.Enumerable => ('[', ']'),
            JsonTypeInfoKind.Dictionary => ('{', '}'),
            _ => (' ', ' '),
        };

        if (startChar == endChar)
        {
            return $"JSON value kind not supported: {typeInfo.Kind}";
        }

        JsonException? lastException = null;
        while (!content.IsEmpty)
        {
            var start = content.IndexOf(startChar);
            if (start < 0)
                return lastException?.Message ?? $"Invalid JSON: '{startChar}' not found";

            var end = content.LastIndexOf(endChar);
            if (end < start)
                return lastException?.Message ?? $"Invalid JSON: '{endChar}' not found or comes before '{startChar}'";

            try
            {
                var result = JsonSerializer.Deserialize(content[start..(end + 1)], typeInfo);
                return result is not null
                    ? result
                    : "Invalid JSON: value is null";
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
