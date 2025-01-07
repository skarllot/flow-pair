using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;

namespace Raiqub.LlmTools.FlowPair.Agent.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
[JsonSerializable(typeof(FilePathResponse))]
public partial class AgentJsonContext : JsonSerializerContext;
