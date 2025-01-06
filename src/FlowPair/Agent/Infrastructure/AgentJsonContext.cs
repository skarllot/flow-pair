using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;

namespace Ciandt.FlowTools.FlowPair.Agent.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ImmutableList<ReviewerFeedbackResponse>))]
[JsonSerializable(typeof(FilePathResponse))]
public partial class AgentJsonContext : JsonSerializerContext;
