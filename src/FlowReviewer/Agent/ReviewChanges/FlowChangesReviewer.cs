using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges.v1;
using Ciandt.FlowTools.FlowReviewer.ChangeTracking;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges;

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
            .ToImmutableList();

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

        var content = result.Unwrap().Content;
        var feedback = content.Contains('[') && content.Contains(']')
            ? JsonSerializer.Deserialize(
                content.AsSpan()[content.IndexOf('[')..(content.LastIndexOf(']') + 1)],
                jsonContext.ImmutableListReviewerFeedbackResponse) ?? []
            : [];
        return feedback;
    }
}
