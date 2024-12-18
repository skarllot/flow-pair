using System.Text.Json.Serialization;
using FxKit.CompilerServices;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat.v1;

public partial class OpenAiRoleJsonConverter : JsonConverter<OpenAiRole>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(OpenAiRoleJsonConverter))]
[EnumMatch]
public enum OpenAiRole
{
    [JsonPropertyName("system")] System,
    [JsonPropertyName("user")] User,
    [JsonPropertyName("assistant")] Assistant,
    [JsonPropertyName("function")] Function
}
