using System.Collections.Immutable;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Raiqub.LlmTools.FlowPair.Support.Console;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Chats.Services;

public partial interface IChatService;

[GenerateAutomaticInterface]
public sealed class ChatService(
    ChatJsonContext jsonContext,
    IProxyCompleteChatHandler completeChatHandler,
    ITempFileWriter tempFileWriter)
    : IChatService
{
    public Result<TResult, string> Run<TResult>(
        Progress progress,
        LlmModelType llmModelType,
        IChatDefinition<TResult> chatDefinition)
        where TResult : notnull
    {
        return Run(
            progress,
            llmModelType,
            chatDefinition,
            chatDefinition.ChatScript.InitialMessages);
    }

    public Result<TResult, string> Run<TResult>(
        Progress progress,
        LlmModelType llmModelType,
        IChatDefinition<TResult> chatDefinition,
        IReadOnlyList<Message> initialMessages)
        where TResult : notnull
    {
        return progress
            .Columns(
                new SpinnerColumn(Spinner.Known.Star),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new LongRemainingTimeColumn())
            .Start(context => RunInternal(context, llmModelType, chatDefinition, initialMessages));
    }

    private Result<TResult, string> RunInternal<TResult>(
        ProgressContext progressContext,
        LlmModelType llmModelType,
        IChatDefinition<TResult> chatDefinition,
        IEnumerable<Message> initialMessages)
        where TResult : notnull
    {
        var chatScript = chatDefinition.ChatScript;
        var progress = progressContext.AddTask(
            $"Running '{chatScript.Name}'",
            maxValue: chatScript.TotalSteps);

        var workspace = new ChatWorkspace(
        [
            new ChatThread(
                progress,
                llmModelType,
                ChatThread.CreateStopKeyword(),
                [new Message(SenderRole.System, chatScript.SystemInstruction), ..initialMessages],
                chatDefinition)
        ]);

        return chatScript.Instructions
            .TryAggregate(workspace, (ws, i) => ws.RunInstruction(i, completeChatHandler))
            .Do(SaveChatHistory)
            .SelectMany(
                chatWorkspace => chatDefinition.ConvertResult(chatWorkspace)
                    .OkOr("Failed to produce a valid output content"));
    }

    private void SaveChatHistory(ChatWorkspace workspace)
    {
        tempFileWriter.WriteJson(
            $"{DateTime.UtcNow:yyyyMMddHHmmss}-history.json",
            workspace.ChatThreads.Select(t => t.Messages).ToImmutableList(),
            jsonContext.ImmutableListImmutableListMessage);
    }
}
