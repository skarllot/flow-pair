using System.Collections.Immutable;
using FxKit.CompilerServices;

namespace Raiqub.LlmTools.FlowPair.Chats.Models;

[Union]
public partial record Instruction
{
    partial record StepInstruction(string Message)
    {
        public Message ToMessage(string stopKeyword) => new(
            SenderRole.User,
            Message.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));
    }

    partial record MultiStepInstruction(string Preamble, ImmutableList<string> Messages, string Ending)
    {
        public Message ToMessage(int index, string stopKeyword) => new(
            SenderRole.User,
            $"{Preamble}{Messages[index]}{Ending}"
                .Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));
    }

    partial record JsonConvertInstruction(string OutputKey, string Message, string JsonSchema)
    {
        public Message ToMessage(string stopKeyword) => new(
            SenderRole.User,
            $"""
             {Message.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword)}
             ```
             {JsonSchema}
             ```
             """);
    }

    partial record CodeExtractInstruction(string OutputKey, string Message)
    {
        public Message ToMessage(string stopKeyword) => new(
            SenderRole.User,
            Message.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));
    }
}
