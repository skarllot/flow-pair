using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
[JsonSerializable(typeof(ImmutableList<ImmutableList<Message>>))]
public partial class AgentJsonContext : JsonSerializerContext;
