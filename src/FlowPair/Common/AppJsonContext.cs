using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Agent.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Flow.GenerateToken.v1;
using Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Persistence.Models.v1;

namespace Ciandt.FlowTools.FlowPair.Common;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
[JsonSerializable(typeof(UserSession))]
[JsonSerializable(typeof(GenerateTokenRequest))]
[JsonSerializable(typeof(GenerateTokenResponse))]
[JsonSerializable(typeof(OpenAiChatCompletionRequest))]
[JsonSerializable(typeof(OpenAiChatCompletionResponse))]
[JsonSerializable(typeof(AnthropicChatCompletionRequest))]
[JsonSerializable(typeof(AnthropicChatCompletionResponse))]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
public partial class AppJsonContext : JsonSerializerContext;
