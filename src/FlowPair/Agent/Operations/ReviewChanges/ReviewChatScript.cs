using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;

public interface
    IReviewChatScript : IProcessableChatScript<ReviewChangesRequest, ImmutableList<ReviewerFeedbackResponse>>;

public sealed class ReviewChatScript(
    AgentJsonContext jsonContext)
    : IReviewChatScript
{
    private const string JsonResponseKey = "ReviewerFeedbackResponse";

    public string Name => "Code review chat script";

    public ImmutableArray<string> Extensions => KnownFileExtension.All;

    public string SystemInstruction =>
        """
        You are an expert developer, your task is to review a set of changes on Git commits.
        You are given a set of Git patches, containing the filenames and their partial contents. Note that you might not have the full context of the code.
        Only review lines of code which have been changed (added or removed) in the pull request. Other lines are added to provide context but should be ignored in the review.
        Begin your feedback by evaluating the changed code using a risk score similar to a LOGAF score but measured from 0 to 3, where 0 is the lowest risk to the codebase if the code is merged and 3 is the highest risk which would likely break something or be unsafe. Risk score should be described as "0 - Not important", "1 - Low priority adjustments", "2 - Medium priority adjustments" or "3 - High priority adjustments".
        Only provide feedback on critical issues. If the code is already well-written or issues are minor, do not provide any feedback.
        Avoid commenting on breaking functions down into smaller, more manageable functions unless it is a significant problem. Be aware that there will be libraries and techniques used which you might not be familiar with, so do not comment on those unless you are confident that there is a problem.
        """;

    public ImmutableList<Instruction> Instructions =>
    [
        Instruction.MultiStepInstruction.Of(
            "Give feedback to ",
            [
                "improve readability where it can significantly impacts understanding",
                "make code cleaner where it introduces substantial benefits",
                "maximize the performance of the code where there is a clear, impactful improvement",
                "flag any API keys or secrets present in plain text immediately as highest risk",
                "rate the changes based on SOLID principles",
                "apply the principles of DRY, KISS, YAGNI and Clean Code",
                "avoid magic strings and numbers",
                "ensure new code follow existing patterns and structure",
            ],
            $" if applicable; otherwise, reply with \"{IChatScript.StopKeywordPlaceholder}\" when there are no suggestions"),
        Instruction.StepInstruction.Of(
            """
            Ensure the feedback contain the file path and the line number.
            Do not provide positive reinforcement or comments on good decisions. Focus solely on areas that need improvement.
            Ensure the feedback details are brief, concise, and accurate. If there are multiple similar issues, only comment on the most critical.
            """),
        Instruction.StepInstruction.Of(
            """
            Include brief example code snippets in the feedback details for your suggested changes when you're confident your suggestions are improvements.
            Use the same programming language as the file under review. If there are multiple improvements you suggest in the feedback details, use an ordered list to indicate the priority of the changes.
            """),
        Instruction.JsonConvertInstruction.Of(
            JsonResponseKey,
            """
            Format the feedback in a valid JSON format as a list of feedbacks, or "[]" for no feedbacks.
            The "feedback" property can be multiline and include example code snippets.
            The schema of the JSON feedback object must be:
            """,
            ReviewerFeedbackResponse.Schema),
    ];

    public ImmutableList<Message> GetInitialMessages(ReviewChangesRequest input) =>
    [
        new Message(
            SenderRole.User,
            $"""
             Follow below a set of changes for review:
             ============
             {input.Diff}
             ============
             """)
    ];

    public Result<object, string> Parse(string key, string input) => key switch
    {
        JsonResponseKey => JsonContentDeserializer
            .TryDeserialize(input, jsonContext.ImmutableListReviewerFeedbackResponse)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'"
    };

    public Option<ImmutableList<ReviewerFeedbackResponse>> CompileOutputs(ChatWorkspace chatWorkspace) =>
        OutputProcessor.AggregateLists<ReviewerFeedbackResponse>(chatWorkspace, JsonResponseKey);
}
