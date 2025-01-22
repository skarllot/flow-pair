using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Chats.Models;

namespace Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;

public interface IChatScript
{
    public const string StopKeywordPlaceholder = "<NO FEEDBACK>";

    string Name { get; }
    ImmutableArray<string> Extensions { get; }
    string SystemInstruction { get; }
    ImmutableList<Instruction> Instructions { get; }
}

public static class ChatScriptExtensions
{
    public static bool CanHandleFile(this IChatScript chatScript, string filePath) =>
        chatScript.Extensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);

    public static double GetTotalSteps(this IChatScript chatScript) => chatScript.Instructions
        .Aggregate(
            (IEnumerable<double>) [0D],
            (curr, next) => next.Match(
                StepInstruction: _ => curr.Select(v => v + 1),
                MultiStepInstruction: x => Enumerable.Range(0, x.Messages.Count)
                    .Select((_, i) => i == 0 ? curr.First() + 1 : 1),
                JsonConvertInstruction: _ => curr.Select(v => v + 1),
                CodeExtractInstruction: _ => curr.Select(v => v + 1)))
        .Sum();
}
