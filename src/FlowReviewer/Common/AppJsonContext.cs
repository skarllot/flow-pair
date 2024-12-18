using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.AnthropicCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.GenerateToken.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Common;

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
[JsonSerializable(typeof(BedrockChatCompletionRequest))]
[JsonSerializable(typeof(BedrockChatCompletionResponse))]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
public partial class AppJsonContext : JsonSerializerContext;
