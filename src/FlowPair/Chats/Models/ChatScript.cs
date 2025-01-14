using System.Collections.Immutable;

namespace Raiqub.LlmTools.FlowPair.Chats.Models;

public sealed record ChatScript(
    string Name,
    ImmutableArray<string> Extensions,
    string SystemInstruction,
    ImmutableList<Message> InitialMessages,
    ImmutableList<Instruction> Instructions)
{
    public const string StopKeywordPlaceholder = "<NO FEEDBACK>";

    public ChatScript(
        string Name,
        ImmutableArray<string> Extensions,
        string SystemInstruction,
        ImmutableList<Instruction> Instructions)
        : this(Name, Extensions, SystemInstruction, [], Instructions)
    {
    }

    public double TotalSteps => Instructions
        .Aggregate(
            (IEnumerable<double>) [0D],
            (curr, next) => next.Match(
                StepInstruction: _ => curr.Select(v => v + 1),
                MultiStepInstruction: x => Enumerable.Range(0, x.Messages.Count)
                    .Select((_, i) => i == 0 ? curr.First() + 1 : 1),
                JsonConvertInstruction: _ => curr.Select(v => v + 1),
                CodeExtractInstruction: _ => curr.Select(v => v + 1)))
        .Sum();

    public static Option<ChatScript> FindChatScriptForFile(
        IReadOnlyList<ChatScript> scripts,
        string filePath)
    {
        return scripts
            .Reverse()
            .FirstOrDefault(i => i.Extensions.Any(s => filePath.EndsWith(s, StringComparison.OrdinalIgnoreCase)));
    }
}
