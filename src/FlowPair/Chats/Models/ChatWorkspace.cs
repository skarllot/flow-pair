using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;

namespace Ciandt.FlowTools.FlowPair.Chats.Models;

public sealed record ChatWorkspace(
    ImmutableList<ChatThread> ChatThreads)
{
    public Result<ChatWorkspace, string> RunInstruction(
        Instruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return instruction.Match(
            StepInstruction: x => RunStepInstruction(x, completeChatHandler),
            MultiStepInstruction: x => RunMultiStepInstruction(x, completeChatHandler),
            JsonConvertInstruction: x => RunJsonInstruction(x, completeChatHandler));
    }

    private Result<ChatWorkspace, string> RunStepInstruction(
        Instruction.StepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return ChatThreads.AsParallel()
            .Select(thread => thread.RunStepInstruction(instruction, completeChatHandler))
            .Sequence()
            .Select(list => new ChatWorkspace(ChatThreads: list.ToImmutableList()));
    }

    private Result<ChatWorkspace, string> RunMultiStepInstruction(
        Instruction.MultiStepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        if (ChatThreads.Count != 1)
        {
            return "Only one multi-step instruction is supported.";
        }

        return Enumerable.Repeat(ChatThreads[0], instruction.Messages.Count)
            .AsParallel()
            .Select((thread, i) => thread.RunMultiStepInstruction(instruction, i, completeChatHandler))
            .Sequence()
            .Select(list => new ChatWorkspace(ChatThreads: list.ToImmutableList()));
    }

    private Result<ChatWorkspace, string> RunJsonInstruction(
        Instruction.JsonConvertInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return ChatThreads.AsParallel()
            .Select(thread => thread.RunJsonInstruction(instruction, completeChatHandler))
            .Sequence()
            .Select(list => new ChatWorkspace(ChatThreads: list.ToImmutableList()));
    }
}
