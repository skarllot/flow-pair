using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Flow.Operations.GenerateToken.v1;
using Ciandt.FlowTools.FlowPair.Flow.Operations.OpenAiCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(GenerateTokenRequest))]
[JsonSerializable(typeof(GenerateTokenResponse))]
[JsonSerializable(typeof(OpenAiChatCompletionRequest))]
[JsonSerializable(typeof(OpenAiChatCompletionResponse))]
[JsonSerializable(typeof(AnthropicChatCompletionRequest))]
[JsonSerializable(typeof(AnthropicChatCompletionResponse))]
public partial class FlowJsonContext : JsonSerializerContext;
