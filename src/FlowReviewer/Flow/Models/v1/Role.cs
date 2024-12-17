using System.Text.Json.Serialization;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

//[JsonConverterGenerator(AllowIntegerValues = false)]
[EnumGenerator]
[JsonConverter(typeof(RoleJsonConverter))]
public enum Role
{
    [JsonPropertyName("system")] System,
    [JsonPropertyName("user")] User,
    [JsonPropertyName("assistant")] Assistant,
    [JsonPropertyName("function")] Function
}
