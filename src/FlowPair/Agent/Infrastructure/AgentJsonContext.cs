using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
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
[JsonSerializable(typeof(CreateUnitTestResponse))]
[JsonSerializable(typeof(UpdateUnitTestResponse))]
public partial class AgentJsonContext : JsonSerializerContext;
