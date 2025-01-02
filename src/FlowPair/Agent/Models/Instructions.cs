using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using FxKit.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

[Union]
public partial record Instruction
{
    partial record StepInstruction(string Message)
    {
        public Message ToMessage(string stopKeyword) => new(
            Role.User,
            Message.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));
    }

    partial record MultiStepInstruction(string Preamble, ImmutableList<string> Messages, string Ending)
    {
        public Message ToMessage(int index, string stopKeyword) => new(
            Role.User,
            $"{Preamble}{Messages[index]}{Ending}"
                .Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));
    }

    partial record JsonConvertInstruction(string OutputKey, string Message, string JsonSchema)
    {
        public Message ToMessage(string stopKeyword) => new(
            Role.User,
            $"""
             {Message.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword)}
             ```
             {JsonSchema}
             ```
             """);
    }
}
