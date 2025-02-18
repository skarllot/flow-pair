using System.Text.Json.Serialization;
using FxKit.CompilerServices;
using Raiqub.Generators.EnumUtilities;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;

public partial class AnthropicRoleJsonConverter : JsonConverter<AnthropicRole>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AnthropicRoleJsonConverter))]
[EnumMatch]
public enum AnthropicRole
{
    [JsonPropertyName("user")] User = 1,
    [JsonPropertyName("assistant")] Assistant,
}
