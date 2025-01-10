using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Chats.Models;

public sealed record ChatThread(
    ProgressTask Progress,
    LlmModelType ModelType,
    string StopKeyword,
    ImmutableList<Message> Messages,
    IMessageParser MessageParser,
    ImmutableDictionary<string, object>? Outputs = null)
{
    private const int MaxJsonRetries = 3;

    public ImmutableDictionary<string, object> Outputs { get; init; } =
        Outputs ?? ImmutableDictionary<string, object>.Empty;

    public Message? LastMessage => Messages.Count > 0 ? Messages[^1] : null;

    private bool IsInterrupted =>
        LastMessage?.Role == SenderRole.Assistant &&
        LastMessage.Content.Contains(StopKeyword, StringComparison.Ordinal);

    public static string CreateStopKeyword() => $"<{Guid.NewGuid().ToString("N")[..8]}>";

    public Result<ChatThread, string> RunStepInstruction(
        Instruction.StepInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return
            (IsInterrupted
                ? this
                : AddMessages(instruction.ToMessage(StopKeyword))
                    .CompleteChat(completeChatHandler))
            .DoBoth(_ => Progress.Increment(1));
    }

    public Result<ChatThread, string> RunMultiStepInstruction(
        Instruction.MultiStepInstruction instruction,
        int index,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return
            (IsInterrupted
                ? this
                : AddMessages(instruction.ToMessage(index, StopKeyword))
                    .CompleteChat(completeChatHandler))
            .DoBoth(_ => Progress.Increment(1));
    }

    public Result<ChatThread, string> RunJsonInstruction(
        Instruction.JsonConvertInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return
            (IsInterrupted
                ? this
                : Enumerable.Range(0, MaxJsonRetries)
                    .TryAggregate(
                        AddMessages(instruction.ToMessage(StopKeyword)),
                        (chat, _) => chat.CompleteChatAndDeserialize(instruction.OutputKey, completeChatHandler)))
            .DoBoth(_ => Progress.Increment(1));
    }

    public Result<ChatThread, string> RunCodeInstruction(
        Instruction.CodeExtractInstruction instruction,
        IProxyCompleteChatHandler completeChatHandler)
    {
        return
            (IsInterrupted
                ? this
                : Enumerable.Range(0, MaxJsonRetries)
                    .TryAggregate(
                        AddMessages(instruction.ToMessage(StopKeyword)),
                        (chat, _) => chat.CompleteChatAndDeserialize(instruction.OutputKey, completeChatHandler)))
            .DoBoth(_ => Progress.Increment(1));
    }

    private ChatThread AddMessages(params ReadOnlySpan<Message> newMessages) =>
        this with { Messages = [..Messages, ..newMessages] };

    private Result<ChatThread, string> CompleteChat(
        IProxyCompleteChatHandler completeChatHandler)
    {
        return completeChatHandler.ChatCompletion(ModelType, Messages)
            .Match<Result<ChatThread, string>>(
                msg => this with { Messages = Messages.Add(msg) },
                error => error.ToString());
    }

    private Result<ChatThread, string> CompleteChatAndDeserialize(
        string outputKey,
        IProxyCompleteChatHandler completeChatHandler)
    {
        if (Outputs.ContainsKey(outputKey))
        {
            return this;
        }

        return (from message in completeChatHandler.ChatCompletion(ModelType, Messages)
                select MessageParser.Parse(outputKey, message.Content)
                    .Match(
                        v => this with { Messages = Messages.Add(message), Outputs = Outputs.Add(outputKey, v) },
                        e => AddMessages(message, new Message(SenderRole.User, e))))
            .MapErr(error => error.ToString());
    }
}
