using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Services;

public partial interface IChatService;

[GenerateAutomaticInterface]
public sealed class ChatService(
    AgentJsonContext jsonContext,
    IProxyCompleteChatHandler completeChatHandler,
    ITempFileWriter tempFileWriter)
    : IChatService
{
    public Result<TResult, string> Run<TResult>(
        Progress progress,
        AllowedModel model,
        IChatDefinition<TResult> chatDefinition,
        IEnumerable<Message> initialMessages)
        where TResult : notnull
    {
        return progress.Start(
            context => RunInternal(context, model, chatDefinition, initialMessages));
    }

    private Result<TResult, string> RunInternal<TResult>(
        ProgressContext progressContext,
        AllowedModel model,
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
                model,
                $"<{Guid.NewGuid().ToString("N")[..8]}>",
                chatDefinition,
                [new Message(Role.System, chatScript.SystemInstruction), ..initialMessages])
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
