using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public sealed record ChatThread(
    ProgressTask Progress,
    AllowedModel Model,
    string StopKeyword,
    Func<ReadOnlySpan<char>, Result<Unit, string>> ValidateJson,
    ImmutableList<Message> Messages)
{
    private const int MaxJsonRetries = 3;

    public Message? LastMessage => Messages.Count > 0 ? Messages[^1] : null;

    public bool IsInterrupted =>
        LastMessage?.Role == Role.Assistant &&
        LastMessage.Content.Contains(StopKeyword, StringComparison.Ordinal);

    public bool IsCompleted => LastMessage?.Role == Role.Assistant;

    public ChatThread AddMessages(params ReadOnlySpan<Message> newMessages) =>
        this with { Messages = [..Messages, ..newMessages] };

    public Result<ChatThread, string> RunStepInstruction(
        Instruction.StepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        try
        {
            if (IsInterrupted)
            {
                return this;
            }

            return AddMessages(instruction.ToMessage(StopKeyword))
                .CompleteChat(completeChatHandler);
        }
        finally
        {
            Progress.Increment(1);
        }
    }

    public Result<ChatThread, string> RunMultiStepInstruction(
        Instruction.MultiStepInstruction instruction,
        int index,
        IProxyCompleteChatHandler completeChatHandler)
    {
        try
        {
            if (IsInterrupted)
            {
                return this;
            }

            return AddMessages(instruction.ToMessage(index, StopKeyword))
                .CompleteChat(completeChatHandler);
        }
        finally
        {
            Progress.Increment(1);
        }
    }

    public Result<ChatThread, string> RunJsonInstruction(
        Instruction.JsonConvertInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        try
        {
            if (IsInterrupted)
            {
                return this;
            }

            return Enumerable.Range(0, MaxJsonRetries)
                .TryAggregate(
                    AddMessages(instruction.ToMessage(StopKeyword)),
                    (chat, _) => chat.CompleteChatAndDeserialize(completeChatHandler));
        }
        finally
        {
            Progress.Increment(1);
        }
    }

    private Result<ChatThread, string> CompleteChat(
        IProxyCompleteChatHandler completeChatHandler)
    {
        return completeChatHandler.ChatCompletion(Model, Messages)
            .Match<Result<ChatThread, string>>(
                msg => this with { Messages = Messages.Add(msg) },
                error => error.ToString());
    }

    private Result<ChatThread, string> CompleteChatAndDeserialize(
        IProxyCompleteChatHandler completeChatHandler)
    {
        if (IsCompleted)
        {
            return this;
        }

        return (from message in completeChatHandler.ChatCompletion(Model, Messages)
                select ValidateJson(message.Content)
                    .Match(
                        _ => AddMessages(message),
                        e => AddMessages(message, new Message(Role.User, e))))
            .MapErr(error => error.ToString());
    }
}
