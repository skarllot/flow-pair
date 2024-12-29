using System.Collections.Immutable;
using System.Text.Json.Serialization.Metadata;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Common;
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
    public Result<ImmutableList<TResult>, string> RunMultiple<TResult>(
        Progress progress,
        AllowedModel model,
        ChatScript chatScript,
        IEnumerable<Message> initialMessages,
        JsonTypeInfo<ImmutableList<TResult>> jsonTypeInfo)
        where TResult : notnull
    {
        return progress.Start(
            context => RunMultipleInternal(context, model, chatScript, initialMessages, jsonTypeInfo));
    }

    private Result<ImmutableList<TResult>, string> RunMultipleInternal<TResult>(
        ProgressContext progressContext,
        AllowedModel model,
        ChatScript chatScript,
        IEnumerable<Message> initialMessages,
        JsonTypeInfo<ImmutableList<TResult>> jsonTypeInfo)
        where TResult : notnull
    {
        var progress = progressContext.AddTask(
            $"Running '{chatScript.Name}'",
            maxValue: chatScript.TotalSteps);

        var workspace = new ChatWorkspace(
        [
            new ChatThread(
                progress,
                model,
                $"<{Guid.NewGuid().ToString("N")[..8]}>",
                str => ContentDeserializer.TryDeserialize(str, jsonTypeInfo).Select(static _ => Unit()),
                [new Message(Role.System, chatScript.SystemInstruction), ..initialMessages])
        ]);

        return chatScript.Instructions
            .TryAggregate(workspace, (ws, i) => ws.RunInstruction(i, completeChatHandler))
            .Do(SaveChatHistory)
            .Select(chatWorkspace => DeserializeList(chatWorkspace, jsonTypeInfo));
    }

    private void SaveChatHistory(ChatWorkspace workspace)
    {
        tempFileWriter.WriteJson(
            $"{DateTime.UtcNow:yyyyMMddHHmmss}-history.json",
            workspace.ChatThreads.Select(t => t.Messages).ToImmutableList(),
            jsonContext.ImmutableListImmutableListMessage);
    }

    private static ImmutableList<TResult> DeserializeList<TResult>(
        ChatWorkspace chatWorkspace,
        JsonTypeInfo<ImmutableList<TResult>> jsonTypeInfo) =>
        chatWorkspace.ChatThreads
            .Where(t => t is { IsInterrupted: false, IsCompleted: true })
            .SelectMany(
                t => ContentDeserializer
                    .TryDeserialize(t.LastMessage?.Content, jsonTypeInfo)
                    .UnwrapOr([]))
            .ToImmutableList();
}
