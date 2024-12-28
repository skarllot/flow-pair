using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public sealed record ChatScript(
    string Name,
    ImmutableArray<string> Extensions,
    string SystemInstruction,
    ImmutableList<Instruction> Instructions)
{
    public const string StopKeywordPlaceholder = "<NO FEEDBACK>";

    public static Option<ChatScript> FindChatScriptForFile(
        IReadOnlyList<ChatScript> scripts,
        string filePath)
    {
        return scripts
            .Reverse()
            .FirstOrDefault(i => i.Extensions.Any(s => filePath.EndsWith(s, StringComparison.OrdinalIgnoreCase)));
    }
}
