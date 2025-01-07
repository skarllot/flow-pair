using System.Text.Json.Serialization;
using Raiqub.LlmTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;
using Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken.v1;
using Raiqub.LlmTools.FlowPair.Flow.Operations.OpenAiCompleteChat.v1;

namespace Raiqub.LlmTools.FlowPair.Flow.Infrastructure;

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
