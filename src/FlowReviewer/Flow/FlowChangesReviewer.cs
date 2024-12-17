using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.ChangeTracking;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.Flow;

public partial interface IFlowChangesReviewer;

[GenerateAutomaticInterface]
public sealed class FlowChangesReviewer(
    IAnsiConsole console,
    ILlmClient llmClient,
    AppJsonContext jsonContext,
    IFileSystem fileSystem)
    : IFlowChangesReviewer
{
    public Option<Unit> Run(ImmutableList<FileChange> changes)
    {
        var result = changes
            .Where(f => f.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .Traverse(
                f =>
                {
                    console.Write($"Reviewing '{f.Path}'...");
                    var message = llmClient.ChatCompletion([s_systemMessage, new Message(Role.User, f.Diff)]);
                    console.WriteLine(" OK");
                    return message;
                });

        if (!result.IsOk)
        {
            console.MarkupLineInterpolated($"[bold red]Error:[/] {result.UnwrapErr().ToString()}");
            return None;
        }

        var feedback = result.Unwrap()
            .Select(m => m.Content)
            .Select(
                m => JsonSerializer.Deserialize(
                    m.AsSpan()[m.IndexOf('[')..(m.LastIndexOf(']') + 1)],
                    jsonContext.ImmutableListReviewerFeedbackResponse) ?? [])
            .Aggregate(ImmutableList<ReviewerFeedbackResponse>.Empty, (curr, next) => curr.AddRange(next));

        console.WriteLine($"Created {feedback.Count} comments");

        if (feedback.Count > 0)
        {
            var feedbackFilePath = fileSystem.Path.Combine(
                fileSystem.Directory.GetCurrentDirectory(),
                $"{DateTime.UtcNow:yyyyMMddHHmmss}-feedback.json");

            using var stream = fileSystem.File.Open(feedbackFilePath, FileMode.Create);
            JsonSerializer.Serialize(stream, feedback, jsonContext.ImmutableListReviewerFeedbackResponse);
        }

        return Unit();
    }

    private static readonly Message s_systemMessage = new(
        Role.System,
        """
        You are an expert developer, your task is to review a set of pull requests on Azure DevOps.

        You are given a JSON return by the Azure DevOps API diff, containing a list of filenames and their partial contents. Note that you might not have the full context of the code.

        Only review lines of code which have been changed (added or removed) in the pull request. Lines which have been removed have the type `REMOVED` and lines which have been added have the type `ADDED`. Other lines are added to provide context but should be ignored in the review.

        Begin your feedback by evaluating the changed code using a risk score similar to a LOGAF score but measured from 0 to 3, where 0 is the lowest risk to the codebase if the code is merged and 3 is the highest risk which would likely break something or be unsafe. Risk score should be described as "0 - Not important", "1 - Low priority adjustments", "2 - Medium priority adjustments" or "3 - High priority adjustments".

        In your feedback:
        1. Focus exclusively on highlighting potential bugs.
        2. Improve readability only if it significantly impacts understanding.
        3. Make code cleaner only if it introduces substantial benefits.
        4. Maximize the performance of the code if there is a clear, impactful improvement.
        5. Flag any API keys or secrets present in plain text immediately as highest risk.
        6. Rate the changes based on SOLID principles if applicable.
        7. Apply the principles of DRY, KISS, YAGNI and Clean Code during the review of the code.

        Only provide feedback on critical issues. If the code is already well-written or issues are minor, do not provide any feedback.

        Avoid commenting on breaking functions down into smaller, more manageable functions unless it is a significant problem. Be aware that there will be libraries and techniques used which you might not be familiar with, so do not comment on those unless you are confident that there is a problem.

        Do not provide positive reinforcement or comments on good decisions. Focus solely on areas that need improvement.

        Your feedbacks will be input in Azure DevOps via API `/comments` endpoint. The feedbacks should be in a valid JSON format.

        Use markdown formatting for the feedback details. Do not include the filename or risk level in the feedback details.

        Ensure the feedback details are brief, concise, and accurate. If there are multiple similar issues, only comment on the most critical.

        Include brief example code snippets in the feedback details for your suggested changes when you're confident your suggestions are improvements. Use the same programming language as the file under review. If there are multiple improvements you suggest in the feedback details, use an ordered list to indicate the priority of the changes.

        It is not necessary to add low-risk comments that are not relevant to changes in the pull request.

        The message in the field text must be in English.

        Format the response in a valid JSON format as a list of feedbacks. The feedback must not contains any block formatting and line breakers. Remember it is crucial that the result has the file path.
        This valid JSON is going to be inserted in a value of a key-value from another JSON object, be-aware about the formatting. Remember to only list feedbacks that needs user action.
        The schema of the JSON feedback object must be:
        ```json
        [
          {
                "riskScore": 0,
                "riskDescription": "Not important",
                "feedback": "",
                "path": "/path/path/file.extension",
                "line": 16,
                "lineType": "ADDED"
            },
            {
                "riskScore": 1,
                "riskDescription": "Low priority adjustments",
                "feedback": "",
                "path": "/path/path/file.extension",
                "line": 20,
                "lineType": "ADDED"
            }
        ]
        ```
        """);
}
