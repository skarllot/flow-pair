namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

public sealed record ReviewerFeedbackResponse(
    int RiskScore,
    string RiskDescription,
    string Feedback,
    string Path,
    int Line,
    string LineType);
