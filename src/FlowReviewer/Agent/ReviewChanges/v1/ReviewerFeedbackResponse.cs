namespace Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges.v1;

public sealed record ReviewerFeedbackResponse(
    int RiskScore,
    string RiskDescription,
    string Feedback,
    string Path,
    int Line,
    string LineType);
