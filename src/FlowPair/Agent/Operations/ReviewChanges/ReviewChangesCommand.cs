using System.Collections.Immutable;
using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Support.Presentation;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;

public sealed class ReviewChangesCommand(
    IAnsiConsole console,
    IReviewChatDefinition chatDefinition,
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
    [Command("review")]
    public int Execute(
        [Argument] string? path = null,
        string? commit = null)
    {
        return (from diff in getChangesHandler.Extract(path, commit).OkOr(0)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 1)
                from feedback in BuildFeedback(diff)
                select 0)
            .UnwrapEither();
    }

    private Result<Unit, int> BuildFeedback(ImmutableList<FileChange> changes)
    {
        var feedback = changes
            .GroupBy(c => ChatScript.FindChatScriptForFile([chatDefinition.ChatScript], c.Path))
            .Where(g => g.Key.IsSome)
            .Select(g => new { Script = g.Key.Unwrap(), Diff = g.AggregateToStringLines(c => c.Diff) })
            .SelectMany(x => GetFeedback(x.Diff))
            .Where(f => !string.IsNullOrWhiteSpace(f.Feedback))
            .OrderByDescending(x => x.RiskScore).ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .ToImmutableList();

        console.WriteLine($"Created {feedback.Count} comments");

        if (feedback.Count > 0)
        {
            tempFileWriter
                .Write(
                    filename: $"{DateTime.UtcNow:yyyyMMddHHmmss}-feedback.html",
                    content: new FeedbackHtmlTemplate(feedback).TransformText())
                .LaunchFile(console);
        }

        return Unit();
    }

    private ImmutableList<ReviewerFeedbackResponse> GetFeedback(
        string diff)
    {
        return chatService.Run(
                console.Progress(),
                LlmModelType.Claude35Sonnet,
                chatDefinition,
                [new Message(SenderRole.User, diff)])
            .DoErr(error => console.MarkupLineInterpolated($"[red]Error:[/] {error}"))
            .UnwrapOrElse(static () => []);
    }
}
