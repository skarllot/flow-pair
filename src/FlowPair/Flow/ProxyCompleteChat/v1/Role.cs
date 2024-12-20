using System.Text.Json.Serialization;
using FxKit.CompilerServices;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat.v1;

public partial class RoleJsonConverter : JsonConverter<Role>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(RoleJsonConverter))]
[EnumMatch]
public enum Role
{
    [JsonPropertyName("system")] System,
    [JsonPropertyName("user")] User,
    [JsonPropertyName("assistant")] Assistant,
    [JsonPropertyName("function")] Function
}
