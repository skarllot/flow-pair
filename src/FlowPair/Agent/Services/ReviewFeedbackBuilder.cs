using System.Collections.Immutable;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Support.Presentation;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Services;

public partial interface IReviewFeedbackBuilder;

[GenerateAutomaticInterface]
public sealed class ReviewFeedbackBuilder(
    TimeProvider timeProvider,
    IAnsiConsole console,
    IReviewChatScript chatScript,
    IChatService chatService,
    ITempFileWriter tempFileWriter)
    : IReviewFeedbackBuilder
{
    public Option<Unit> Run(ImmutableList<FileChange> changes)
    {
        var aggregatedChanges = changes
            .Where(c => chatScript.CanHandleFile(c.Path))
            .AggregateToStringLines(c => c.Diff);

        var feedback = chatService
            .Run(
                new ReviewChangesRequest(aggregatedChanges),
                console.Progress(),
                LlmModelType.Claude35Sonnet,
                chatScript)
            .DoErr(error => console.MarkupLineInterpolated($"[red]Error:[/] {error}"))
            .UnwrapOrElse(static () => [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Feedback))
            .OrderByDescending(x => x.RiskScore).ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .ToImmutableList();

        console.WriteLine($"Created {feedback.Count} comments");

        if (feedback.Count > 0)
        {
            tempFileWriter
                .Write(
                    filename: $"{timeProvider.GetUtcNow():yyyyMMddHHmmss}-feedback.html",
                    content: new FeedbackHtmlTemplate(feedback).TransformText())
                .LaunchFile(console);
        }

        return Unit();
    }
}
