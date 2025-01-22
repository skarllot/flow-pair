using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;

public interface IProcessableChatScript<in TInput, TResult> : IChatScript, IMessageParser
    where TInput : notnull
    where TResult : notnull
{
    ImmutableList<Message> GetInitialMessages(TInput input);

    Option<TResult> CompileOutputs(ChatWorkspace chatWorkspace);
}

public static class ProcessableChatScriptExtensions
{
    public static ChatWorkspace CreateChatWorkspace<TInput, TResult>(
        this IProcessableChatScript<TInput, TResult> chatScript,
        TInput input,
        ProgressTask progress,
        LlmModelType llmModelType)
        where TInput : notnull
        where TResult : notnull
    {
        return new ChatWorkspace(
        [
            new ChatThread(
                Progress: progress,
                ModelType: llmModelType,
                StopKeyword: ChatThread.CreateStopKeyword(),
                Messages:
                [
                    new Message(SenderRole.System, chatScript.SystemInstruction),
                    ..chatScript.GetInitialMessages(input),
                ],
                MessageParser: chatScript),
        ]);
    }
}
