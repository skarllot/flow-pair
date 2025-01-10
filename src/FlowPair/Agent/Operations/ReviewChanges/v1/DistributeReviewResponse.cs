using System.Collections.Immutable;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;

public sealed record DistributeReviewResponse(
    ImmutableList<string> Contexts);

public sealed record ReviewContext(
    string Description,
    ImmutableList<string> Files);
