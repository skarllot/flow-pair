using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Common;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
[JsonSerializable(typeof(UserSession))]
[JsonSerializable(typeof(GenerateTokenRequest))]
[JsonSerializable(typeof(GenerateTokenResponse))]
[JsonSerializable(typeof(ImmutableList<GetAvailableModelsResponse>))]
[JsonSerializable(typeof(ChatCompletionRequest))]
[JsonSerializable(typeof(ChatCompletionResponse))]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
public partial class AppJsonContext : JsonSerializerContext;
