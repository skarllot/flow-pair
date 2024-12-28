using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Git.GetChanges;
using Ciandt.FlowTools.FlowPair.Support.Persistence;
using Ciandt.FlowTools.FlowPair.Support.Presentation;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public sealed class ReviewChangesCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    AgentJsonContext jsonContext,
    IGitGetChangesHandler getChangesHandler,
    ILoginUseCase loginUseCase,
    IChatService chatService)
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
            .GroupBy(c => ChatScript.FindChatScriptForFile(ReviewChatScript.Default, c.Path))
            .Where(g => g.Key.IsSome)
            .Select(g => new { Script = g.Key.Unwrap(), Diff = g.AggregateToStringLines(c => c.Diff) })
            .SelectMany(x => GetFeedback(x.Diff, x.Script))
            .Where(f => !string.IsNullOrWhiteSpace(f.Feedback))
            .OrderByDescending(x => x.RiskScore).ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .ToImmutableList();

        console.WriteLine($"Created {feedback.Count} comments");

        if (feedback.Count > 0)
        {
            var tempPath = ApplicationData.GetTempPath(fileSystem);
            tempPath.Create();

            var feedbackFilePath = tempPath.NewFile($"{DateTime.UtcNow:yyyyMMddHHmmss}-feedback.html");

            var htmlContent = new FeedbackHtmlTemplate(feedback).TransformText();
            feedbackFilePath.WriteAllText(htmlContent, Encoding.UTF8);

            FileLauncher.OpenFile(feedbackFilePath.FullName);
        }

        return Unit();
    }

    private ImmutableList<ReviewerFeedbackResponse> GetFeedback(
        string diff,
        ChatScript chatScript)
    {
        return chatService.RunMultiple(
                console.Progress(),
                AllowedModel.Claude35Sonnet,
                chatScript,
                [new Message(Role.User, diff)],
                jsonContext.ImmutableListReviewerFeedbackResponse)
            .DoErr(error => console.MarkupLineInterpolated($"[red]Error:[/] {error}"))
            .UnwrapOrElse(static () => []);
    }
}
