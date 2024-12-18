using System.Text.Json.Serialization;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowReviewer.Flow.AnthropicCompleteChat.v1;

public partial class AllowedAnthropicModelsJsonConverter : JsonConverter<AllowedAnthropicModels>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AllowedAnthropicModelsJsonConverter))]
public enum AllowedAnthropicModels
{
    [JsonPropertyName("anthropic.claude-3-sonnet")] Claude3Sonnet,
    [JsonPropertyName("anthropic.claude-35-sonnet")] Claude35Sonnet,
}
