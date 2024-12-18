using System.Text.Json.Serialization;
using FxKit.CompilerServices;
using Raiqub.Generators.EnumUtilities;

namespace Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

public partial class AllowedModelJsonConverter : JsonConverter<AllowedModel>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AllowedModelJsonConverter))]
[EnumMatch]
public enum AllowedModel
{
    [JsonPropertyName("gpt-4")] Gpt4,
    [JsonPropertyName("gpt-4o")] Gpt4o,
    [JsonPropertyName("gpt-4o-mini")] Gpt4oMini,
    [JsonPropertyName("gpt-35-turbo")] Gpt35Turbo,
    [JsonPropertyName("text-embedding-ada-002")] TextEmbeddingAda002,
    [JsonPropertyName("textembedding-gecko@003")] TextEmbeddingGecko003,
    [JsonPropertyName("gemini-1.5-flash")] Gemini15Flash,
    [JsonPropertyName("gemini-1.5-pro")] Gemini15Pro,
    [JsonPropertyName("anthropic.claude-3-sonnet")] Claude3Sonnet,
    [JsonPropertyName("anthropic.claude-35-sonnet")] Claude35Sonnet,
    [JsonPropertyName("meta.llama3-70b-instruct")] Llama370b
}
