using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Agent.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.ChangeTracking;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.Support.Persistence;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.ReviewChanges;

public partial interface IFlowChangesReviewer;

[GenerateAutomaticInterface]
public sealed class FlowChangesReviewer(
    IAnsiConsole console,
    IProxyClient proxyClient,
    AppJsonContext jsonContext,
    IFileSystem fileSystem)
    : IFlowChangesReviewer
{
    public Option<Unit> Run(ImmutableList<FileChange> changes)
    {
        var feedback = changes
            .GroupBy(c => Instructions.FindInstructionsForFile(Instructions.Default, c.Path))
            .Where(g => g.Key.IsSome)
            .Select(g => new { Instructions = g.Key.Unwrap(), Diff = g.AggregateToStringLines(c => c.Diff) })
            .SelectMany(x => GetFeedback([AllowedModel.Claude35Sonnet, AllowedModel.Gpt4o], x.Diff, x.Instructions))
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

            OpenHtmlFile(feedbackFilePath.FullName);
        }

        return Unit();
    }

    private static void OpenHtmlFile(string filePath)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo { FileName = filePath, UseShellExecute = true };
        process.Start();
    }

    private ImmutableList<ReviewerFeedbackResponse> GetFeedback(
        IEnumerable<AllowedModel> allowedModels,
        string diff,
        Instructions instructions)
    {
        return allowedModels
            .SelectMany(x => GetFeedback(x, diff, instructions))
            .ToImmutableList();
    }

    private ImmutableList<ReviewerFeedbackResponse> GetFeedback(
        AllowedModel model,
        string diff,
        Instructions instructions)
    {
        var result = proxyClient.ChatCompletion(
            model,
            [new Message(Role.System, instructions.Message), new Message(Role.User, diff)]);

        if (!result.IsOk)
        {
            console.MarkupLineInterpolated($"[bold red]Error:[/] {result.UnwrapErr().ToString()}");
            return [];
        }

        var feedback = TryDeserializeFeedback(result.Unwrap().Content);
        return feedback;
    }

    private ImmutableList<ReviewerFeedbackResponse> TryDeserializeFeedback(ReadOnlySpan<char> content)
    {
        if (content.IsWhiteSpace())
            return [];

        while (!content.IsEmpty)
        {
            var start = content.IndexOf('[');
            if (start < 0)
                return [];

            var end = content.IndexOf(']');
            if (end < start)
                return [];

            try
            {
                return JsonSerializer.Deserialize(
                    content[start..(end + 1)],
                    jsonContext.ImmutableListReviewerFeedbackResponse) ?? [];
            }
            catch (JsonException)
            {
                content = content[(end + 1)..];
            }
        }

        return [];
    }
}
