using System.Collections.Immutable;
using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Support.Presentation;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;

public sealed class ReviewChangesCommand(
    TimeProvider timeProvider,
    IAnsiConsole console,
    IReviewChatScript chatScript,
    IGitGetChangesHandler getChangesHandler,
    ILoginUseCase loginUseCase,
    IChatService chatService,
    ITempFileWriter tempFileWriter)
{
    /// <summary>
    /// Review changed files using Flow.
    /// </summary>
    /// <param name="path">Path to the repository.</param>
    /// <param name="commit">-c, Commit hash.</param>
    /// <param name="sourceBranch">-sb, The name of the source branch (e.g. main).</param>
    /// <param name="targetBranch">-tb, The name of the target branch (e.g. feature-1).</param>
    [Command("review")]
    public int Execute(
        [Argument] string? path = null,
        string? commit = null,
        string? sourceBranch = null,
        string? targetBranch = null)
    {
        if (commit is not null && (sourceBranch is not null || targetBranch is not null))
        {
            console.MarkupLine("[red]Error:[/] Specify a commit or source/target branch.");
            return 1;
        }

        var extract = sourceBranch is not null || targetBranch is not null
            ? getChangesHandler.ExtractFromBranchesDiff(path, sourceBranch, targetBranch)
            : getChangesHandler.Extract(path, commit);

        return (from diff in extract.OkOr(0)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 1)
                from feedback in BuildFeedback(diff)
                select 0)
            .UnwrapEither();
    }

    private Result<Unit, int> BuildFeedback(ImmutableList<FileChange> changes)
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
