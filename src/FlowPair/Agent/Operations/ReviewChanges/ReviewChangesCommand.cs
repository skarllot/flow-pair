using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Git.GetChanges;
using Ciandt.FlowTools.FlowPair.Support.Persistence;
using Ciandt.FlowTools.FlowPair.Support.Presentation;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public class ReviewChangesCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    AgentJsonContext jsonContext,
    IGitGetChangesHandler getChangesHandler,
    ILoginUseCase loginUseCase,
    IProxyCompleteChatHandler completeChatHandler)
{
    /// <summary>
    /// Review changed files using Flow.
    /// </summary>
    /// <param name="path">Path to the repository.</param>
    [Command("review")]
    public int Execute(
        [Argument] string? path = null)
    {
        return (from diff in getChangesHandler.Extract(path).OkOr(0)
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
            .GroupBy(c => Instructions.FindInstructionsForFile(Instructions.Default, c.Path))
            .Where(g => g.Key.IsSome)
            .Select(g => new { Instructions = g.Key.Unwrap(), Diff = g.AggregateToStringLines(c => c.Diff) })
            .SelectMany(x => GetFeedback(AllowedModel.Claude35Sonnet, x.Diff, x.Instructions))
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
        AllowedModel model,
        string diff,
        Instructions instructions)
    {
        var result = completeChatHandler.ChatCompletion(
            model,
            [new Message(Role.System, instructions.Message), new Message(Role.User, diff)]);

        if (!result.IsOk)
        {
            console.MarkupLineInterpolated($"[bold red]Error:[/] {result.UnwrapErr().ToString()}");
            return [];
        }

        var feedback = ContentDeserializer.TryDeserializeFeedback(
            result.Unwrap().Content,
            jsonContext.ImmutableListReviewerFeedbackResponse);
        return feedback;
    }
}
