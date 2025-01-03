using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Chats.Contracts.v1;
using Ciandt.FlowTools.FlowPair.Chats.Infrastructure;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Chats.Services;

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
        IChatDefinition<TResult> chatDefinition,
        IEnumerable<Message> initialMessages)
        where TResult : notnull
    {
        return progress.Start(
            context => RunInternal(context, llmModelType, chatDefinition, initialMessages));
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
                $"<{Guid.NewGuid().ToString("N")[..8]}>",
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
