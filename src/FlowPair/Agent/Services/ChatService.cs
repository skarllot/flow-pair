using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Services;

public partial interface IChatService;

[GenerateAutomaticInterface]
public sealed class ChatService : IChatService
{
    public Option<TResult> Run<TResult>(Progress progress, ChatScript chatScript, IEnumerable<Message> initialMessages)
        where TResult : notnull
    {
        return progress.Start(context => RunInternal<TResult>(context, chatScript, initialMessages));
    }

    private Option<TResult> RunInternal<TResult>(
        ProgressContext progressContext,
        ChatScript chatScript,
        IEnumerable<Message> initialMessages)
        where TResult : notnull
    {
        var progress = progressContext.AddTask(
            $"Running '{chatScript.Name}'",
            maxValue: CalculateTotalSteps(chatScript));

        var workspace = new ChatWorkspace<TResult>(
        [
            new ChatThread<TResult>(
                progress,
                [new Message(Role.System, chatScript.SystemInstruction), ..initialMessages],
                $"<{Guid.NewGuid().ToString("N")[..8]}>")
        ]);

        var result = chatScript.Instructions
            .Aggregate(workspace, static (ws, i) => ws.RunInstruction(i));
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
}
