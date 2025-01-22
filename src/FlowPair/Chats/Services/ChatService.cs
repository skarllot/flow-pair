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
    TimeProvider timeProvider,
    ChatJsonContext jsonContext,
    IProxyCompleteChatHandler completeChatHandler,
    ITempFileWriter tempFileWriter)
    : IChatService
{
    public Result<TResult, string> Run<TInput, TResult>(
        TInput input,
        Progress progress,
        LlmModelType llmModelType,
        IProcessableChatScript<TInput, TResult> chatScript)
        where TInput : notnull
        where TResult : notnull
    {
        return progress
            .Columns(
                new SpinnerColumn(Spinner.Known.Star),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new LongRemainingTimeColumn())
            .Start(context => RunInternal(input, context, llmModelType, chatScript));
    }

    private Result<TResult, string> RunInternal<TInput, TResult>(
        TInput input,
        ProgressContext progressContext,
        LlmModelType llmModelType,
        IProcessableChatScript<TInput, TResult> chatScript)
        where TInput : notnull
        where TResult : notnull
    {
        var progress = progressContext.AddTask(
            $"Running '{chatScript.Name}'",
            maxValue: chatScript.GetTotalSteps());

        var workspace = chatScript.CreateChatWorkspace(input, progress, llmModelType);

        return chatScript.Instructions
            .TryAggregate(workspace, (ws, i) => ws.RunInstruction(i, completeChatHandler))
            .Do(SaveChatHistory)
            .SelectMany(
                chatWorkspace => chatScript.CompileOutputs(chatWorkspace)
                    .OkOr("Failed to produce a valid output content"));
    }

    private void SaveChatHistory(ChatWorkspace workspace)
    {
        tempFileWriter.WriteJson(
            $"{timeProvider.GetUtcNow():yyyyMMddHHmmss}-history.json",
            workspace.ChatThreads.Select(t => t.Messages).ToImmutableList(),
            jsonContext.ImmutableListImmutableListMessage);
    }
}
