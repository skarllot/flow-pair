using System.Collections.Immutable;
using System.Globalization;
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
    /// <param name="commit">-c, Commit hash</param>
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
            .GroupBy(c => ChatScript.FindChatScriptForFile(ChatScript.Default, c.Path))
            .Where(g => g.Key.IsSome)
            .Select(g => new { Script = g.Key.Unwrap(), Diff = g.AggregateToStringLines(c => c.Diff) })
            .SelectMany(
                x => console.Progress()
                    .Start(c => GetFeedback(c, AllowedModel.Claude35Sonnet, x.Diff, x.Script)))
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
        ProgressContext progressContext,
        AllowedModel model,
        string diff,
        ChatScript chatScript)
    {
        var progress = progressContext.AddTask(
            $"Running '{chatScript.Name}'",
            maxValue: CalculateTotalSteps(chatScript));
        ImmutableList<ImmutableList<Message>> chatThreads =
        [
            [new Message(Role.System, chatScript.SystemInstruction), new Message(Role.User, diff)],
        ];

        var stopKeyword = $"<{Guid.NewGuid().ToString("N")[..8]}>";
        foreach (var instruction in chatScript.Instructions)
        {
            var currThreads = chatThreads;
            var runResult = instruction.Match(
                StepInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads),
                MultiStepInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads),
                JsonConvertInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads));
            if (!runResult.IsSome)
            {
                return [];
            }

            chatThreads = runResult.Unwrap();
        }

        var feedback = MergeFeedback(chatThreads);
        return feedback;
    }

    private static double CalculateTotalSteps(ChatScript chatScript) =>
        chatScript.Instructions
            .Aggregate(
                (IEnumerable<double>) [0D],
                (curr, next) => next.Match(
                    StepInstruction: _ => curr.Select(v => v + 1),
                    MultiStepInstruction: x => Enumerable.Range(0, x.Messages.Count).Select(_ => curr.First() + 1),
                    JsonConvertInstruction: _ => curr.Select(v => v + 1)))
            .Sum();

    private Option<ImmutableList<ImmutableList<Message>>> RunInstruction(
        ProgressTask progress,
        AllowedModel model,
        string stopKeyword,
        Instruction.StepInstruction instruction,
        ImmutableList<ImmutableList<Message>> chatThreads)
    {
        progress.Increment(chatThreads.Count(x => ChatThreadSpec.IsClosed(x, stopKeyword)));

        var newMessage = new Message(
            Role.User,
            instruction.Messsage.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));

        return (from chatThread in chatThreads.AsParallel()
                where !ChatThreadSpec.IsClosed(chatThread, stopKeyword)
                select CompleteChat(model, chatThread.Add(newMessage))
                    .DoAlways(() => progress.Increment(1)))
            .Sequence()
            .Map(l => l.Concat(chatThreads.Where(x => ChatThreadSpec.IsClosed(x, stopKeyword))))
            .Map(l => l.ToImmutableList());
    }

    private Option<ImmutableList<ImmutableList<Message>>> RunInstruction(
        ProgressTask progress,
        AllowedModel model,
        string stopKeyword,
        Instruction.MultiStepInstruction instruction,
        ImmutableList<ImmutableList<Message>> chatThreads)
    {
        if (chatThreads.Count > 1)
        {
            console.MarkupLine("[bold red]Error:[/] Only one multi-step instruction is supported.");
            return None;
        }

        if (ChatThreadSpec.IsClosed(chatThreads[0], stopKeyword))
        {
            return chatThreads;
        }

        var existingThread = chatThreads[0];
        var template = string.Format(
            CultureInfo.InvariantCulture,
            "{0}{{0}}{1}",
            instruction.Preamble.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword),
            instruction.Ending.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));

        return instruction.Messages.AsParallel()
            .Select(
                msg => CompleteChat(
                        model,
                        existingThread.Add(
                            new Message(
                                Role.User,
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    template,
                                    msg.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword)))))
                    .DoAlways(() => progress.Increment(1)))
            .Sequence()
            .Map(l => l.ToImmutableList());
    }

    private Option<ImmutableList<ImmutableList<Message>>> RunInstruction(
        ProgressTask progress,
        AllowedModel model,
        string stopKeyword,
        Instruction.JsonConvertInstruction instruction,
        ImmutableList<ImmutableList<Message>> chatThreads)
    {
        progress.Increment(chatThreads.Count(x => ChatThreadSpec.IsClosed(x, stopKeyword)));

        var newMessage = new Message(Role.User, $"{instruction.Message}\n\n{JsonSchema.FeedbackJsonSchema}");
        return chatThreads
            .Where(t => !ChatThreadSpec.IsClosed(t, stopKeyword))
            .AsParallel()
            .Select(
                t =>
                {
                    if (!CompleteChat(model, t.Add(newMessage)).TryGet(out var newThread))
                    {
                        progress.Increment(1);
                        return None;
                    }

                    var deserializeResult = ContentDeserializer.TryDeserializeFeedback(
                        newThread[^1].Content,
                        jsonContext.ImmutableListReviewerFeedbackResponse);
                    while (!deserializeResult.TryGet(out _, out var error))
                    {
                        if (CompleteChat(model, newThread.Add(new Message(Role.User, error)))
                            .TryGet(out newThread))
                        {
                            progress.Increment(1);
                            return None;
                        }

                        deserializeResult = ContentDeserializer.TryDeserializeFeedback(
                            newThread![^1].Content,
                            jsonContext.ImmutableListReviewerFeedbackResponse);
                    }

                    progress.Increment(1);
                    return Some(newThread);
                })
            .Sequence()
            .Map(l => l.Concat(chatThreads.Where(x => ChatThreadSpec.IsClosed(x, stopKeyword))))
            .Map(l => l.ToImmutableList());
    }

    private Option<ImmutableList<Message>> CompleteChat(AllowedModel model, ImmutableList<Message> chatThread)
    {
        return completeChatHandler.ChatCompletion(model, chatThread)
            .Match(
                msg => Some(chatThread.Add(msg)),
                error =>
                {
                    console.MarkupLineInterpolated($"[bold red]Error:[/] {error.ToString()}");
                    return None;
                });
    }

    private ImmutableList<ReviewerFeedbackResponse> MergeFeedback(ImmutableList<ImmutableList<Message>> chatThreads)
    {
        return chatThreads
            .SelectMany(
                t => ContentDeserializer
                    .TryDeserializeFeedback(t[^1].Content, jsonContext.ImmutableListReviewerFeedbackResponse)
                    .UnwrapOr([]))
            .ToImmutableList();
    }
}
