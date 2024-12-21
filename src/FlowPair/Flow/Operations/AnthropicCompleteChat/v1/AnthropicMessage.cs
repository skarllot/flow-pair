using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;

/// <summary>The input message.</summary>
/// <param name="Role">The role of the conversation turn.</param>
/// <param name="Content">The content of the conversation turn.</param>
public sealed record AnthropicMessage(
    [property: JsonPropertyName("role")] AnthropicRole Role,
    [property: JsonPropertyName("content")] ImmutableList<AnthropicContent> Content);

public partial class AnthropicMessageTypeJsonConverter : JsonConverter<AnthropicMessageType>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AnthropicMessageTypeJsonConverter))]
public enum AnthropicMessageType
{
    [JsonPropertyName("text")] Text,
    [JsonPropertyName("image")] Image,
}

public sealed record AnthropicContent(
    [property: JsonPropertyName("type")] AnthropicMessageType Type,
    [property: JsonPropertyName("text")] string? Text = null,
    [property: JsonPropertyName("source")] AnthropicSource? Source = null);

/// <summary>The content of the conversation turn.</summary>
/// <param name="Type">The encoding type for the image.</param>
/// <param name="MediaType">The type of the image.</param>
/// <param name="Data">The base64 encoded image bytes for the image. The maximum image size is 3.75MB. The maximum height and width of an image is 8000 pixels.</param>
public sealed record AnthropicSource(
    [property: JsonPropertyName("type")] AnthropicSourceType Type,
    [property: JsonPropertyName("media_type")] AnthropicMediaType MediaType,
    [property: JsonPropertyName("data")] string Data);

public partial class AnthropicSourceTypeJsonConverter : JsonConverter<AnthropicSourceType>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AnthropicSourceTypeJsonConverter))]
public enum AnthropicSourceType
{
    [JsonPropertyName("base64")] Base64,
}

public partial class AnthropicMediaTypeJsonConverter : JsonConverter<AnthropicMediaType>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AnthropicMediaTypeJsonConverter))]
public enum AnthropicMediaType
{
    [JsonPropertyName("image/jpeg")] Jpeg,
    [JsonPropertyName("image/png")] Png,
    [JsonPropertyName("image/webp")] WebP,
    [JsonPropertyName("image/gif")] Gif
}
