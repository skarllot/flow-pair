using System.Text.Json.Serialization;
using Raiqub.Generators.EnumUtilities;

namespace Raiqub.LlmTools.FlowPair.Flow.Operations.OpenAiCompleteChat.v1;

public partial class AllowedOpenAiModelsJsonConverter : JsonConverter<AllowedOpenAiModels>;

[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(AllowedOpenAiModelsJsonConverter))]
public enum AllowedOpenAiModels
{
    [JsonPropertyName("gpt-4")] Gpt4,
    [JsonPropertyName("gpt-4o")] Gpt4o,
    [JsonPropertyName("gpt-4o-mini")] Gpt4oMini,
    [JsonPropertyName("gpt-35-turbo")] Gpt35Turbo,
    [JsonPropertyName("text-embedding-ada-002")] TextEmbeddingAda002,
}
