using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public sealed class ChatWorkspace<TResult>(
    ProgressTask progress,
    AllowedModel model,
    string stopKeyword,
    ImmutableList<ChatThread<TResult>> chatThreads)
    where TResult : notnull
{
    public Option<ChatWorkspace<TResult>> RunInstruction(Instruction instruction)
    {
        var runResult = instruction.Match(
            StepInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads),
            MultiStepInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads),
            JsonConvertInstruction: x => RunInstruction(progress, model, stopKeyword, x, currThreads));
    }
}
