using System.Collections.Immutable;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(ChatService))]
public class ChatServiceTest
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IProxyCompleteChatHandler _completeChatHandler = Substitute.For<IProxyCompleteChatHandler>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();
    private readonly ChatService _chatService;
    private readonly IProcessableChatScript<Unit, Unit> _chatScript;
    private readonly Progress _progress = new(AnsiConsole.Create(new AnsiConsoleSettings()));

    public ChatServiceTest()
    {
        _chatService = new ChatService(_timeProvider, ChatJsonContext.Default, _completeChatHandler, _tempFileWriter);

        _chatScript = Substitute.For<IProcessableChatScript<Unit, Unit>>();
        _chatScript.Name.Returns("TestScript");
        _chatScript.SystemInstruction.Returns("System Instruction");
    }

    [Fact]
    public void RunShouldReturnValidFeedbackWhenChatScriptIsValid()
    {
        // Arrange
        const string outputKey = "TestKey";
        _chatScript.GetInitialMessages(Unit())
            .Returns([new Message(SenderRole.User, "Initial Content")]);
        _chatScript.Instructions
            .Returns([new Instruction.JsonConvertInstruction(outputKey, "Step Message", "{}")]);
        _chatScript
            .Parse(outputKey, Arg.Any<string>())
            .Returns(_ => Ok<object, string>(Unit()));
        _chatScript
            .CompileOutputs(Arg.Any<ChatWorkspace>())
            .Returns(
                c => c.Arg<ChatWorkspace>().ChatThreads
                    .Select(t => t.Outputs.Get(outputKey).OfType<Unit>())
                    .Sequence()
                    .SelectMany(l => l.TrySingle().ToOption()));

        _completeChatHandler
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "()"));

        // Act
        var result = _chatService.Run(
            input: Unit(),
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatScript: _chatScript);

        // Assert
        result.Should().BeOk()
            .Should().Be(Unit());
    }

    [Fact]
    public void RunShouldReturnErrorWhenDeserializationFails()
    {
        // Arrange
        _chatScript.GetInitialMessages(Unit())
            .Returns([new Message(SenderRole.User, "Initial Content")]);
        _chatScript.Instructions
            .Returns([new Instruction.StepInstruction("Step Message")]);
        _chatScript
            .CompileOutputs(Arg.Any<ChatWorkspace>())
            .Returns(_ => None);

        _completeChatHandler
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "Invalid Feedback Content"));

        // Act
        var result = _chatService.Run(
            input: Unit(),
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatScript: _chatScript);

        // Assert
        result.Should().BeErr("Failed to produce a valid output content");
    }

    [Fact]
    public void RunShouldSaveChatHistoryWhenExecutionSucceeds()
    {
        // Arrange
        _chatScript.GetInitialMessages(Unit())
            .Returns([new Message(SenderRole.User, "Initial Content")]);
        _chatScript.Instructions
            .Returns([new Instruction.StepInstruction("Step Message")]);
        _chatScript
            .CompileOutputs(Arg.Is<ChatWorkspace>(w => w.ChatThreads[0].LastMessage!.Content == "Feedback Content"))
            .Returns(Unit());

        _completeChatHandler
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "Feedback Content"));

        // Act
        var result = _chatService.Run(
            input: Unit(),
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatScript: _chatScript);

        // Assert
        result.Should().BeOk();

        _tempFileWriter.Received(1).WriteJson(
            Arg.Any<string>(),
            Arg.Any<ImmutableList<ImmutableList<Message>>>(),
            ChatJsonContext.Default.ImmutableListImmutableListMessage);
    }
}
