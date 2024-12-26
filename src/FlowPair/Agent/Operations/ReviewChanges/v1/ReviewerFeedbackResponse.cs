namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;

public sealed record ReviewerFeedbackResponse(
    int RiskScore,
    string RiskDescription,
    string Title,
    string Category,
    string Language,
    string Feedback,
    string Path,
    string LineRange);
