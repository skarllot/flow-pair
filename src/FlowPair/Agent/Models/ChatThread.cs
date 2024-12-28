using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public sealed class ChatThread<TResult>(
    ProgressTask progress,
    AllowedModel model,
    ImmutableList<Message> messages,
    string stopKeyword)
{
    public bool IsClosed =>
        messages[^1].Role == Role.Assistant &&
        messages[^1].Content.Contains(stopKeyword, StringComparison.Ordinal);

    public Result<ChatThread<TResult>, string> RunInstruction(
        Instruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        if (IsClosed)
        {
            return this;
        }

        return from newMessages in instruction.Match(
                StepInstruction: x => RunStepInstruction(x, completeChatHandler),
                MultiStepInstruction: x => RunMultiStepInstruction(x, completeChatHandler),
                JsonConvertInstruction: x => RunJsonInstruction(x, completeChatHandler))
            select new ChatThread<TResult>(progress, model, newMessages, stopKeyword);
    }

    private Result<ImmutableList<Message>, string> RunStepInstruction(
        Instruction.StepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        var newMessage = new Message(
            Role.User,
            instruction.Messsage.Replace(ChatScript.StopKeywordPlaceholder, stopKeyword));

        return CompleteChat(model, completeChatHandler, messages.Add(newMessage));
    }

    private Result<ImmutableList<Message>, string> RunMultiStepInstruction(
        Instruction.MultiStepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
    }

    private Result<ImmutableList<Message>, string> RunJsonInstruction(
        Instruction.JsonConvertInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
    }

    private static Result<ImmutableList<Message>, string> CompleteChat(
        AllowedModel model,
        IProxyCompleteChatHandler completeChatHandler,
        ImmutableList<Message> messages)
    {
        return completeChatHandler.ChatCompletion(model, messages)
            .Match<Result<ImmutableList<Message>, string>>(
                msg => messages.Add(msg),
                error => error.ToString());
    }
}
