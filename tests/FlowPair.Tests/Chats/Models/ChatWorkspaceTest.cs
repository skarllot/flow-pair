using System.Collections.Immutable;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Models;

[TestSubject(typeof(ChatWorkspace))]
public class ChatWorkspaceTest
{
    private readonly ProgressTask _progressTask = new(0, "description", 100);
    private readonly IProxyCompleteChatHandler _handler = Substitute.For<IProxyCompleteChatHandler>();
    private readonly IMessageParser _messageParser = Substitute.For<IMessageParser>();

    public ChatWorkspaceTest()
    {
        _handler
            .ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "Response"));
    }

    [Fact]
    public void RunInstructionShouldHandleAllInstructionTypes()
    {
        // Arrange
        var initialMessages = ImmutableList.Create(new Message(SenderRole.User, "Initial"));
        var workspace = new ChatWorkspace([CreateChatThread(initialMessages), CreateChatThread(initialMessages)]);

        var instructions = new[]
        {
            Instruction.StepInstruction.Of("Step Message"),
            Instruction.JsonConvertInstruction.Of("JsonKey", "JSON Message", "{ \"schema\": \"value\" }"),
            Instruction.CodeExtractInstruction.Of("CodeKey", "Code Message")
        };

        _messageParser
            .Parse(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Unit());

        // Act & Assert
        foreach (var instruction in instructions)
        {
            var result = workspace.RunInstruction(instruction, _handler);

            result.Should().BeOk().ChatThreads.Should().HaveCount(2);
            result.Unwrap().ChatThreads.Should().OnlyContain(t => t.Messages.Count == 3);

            var lastMessage = result.Unwrap().ChatThreads[0].Messages.Last();
            lastMessage.Content.Should().Contain(
                instruction switch
                {
                    Instruction.StepInstruction => "Response",
                    Instruction.JsonConvertInstruction => "Response",
                    Instruction.CodeExtractInstruction => "Response",
                    _ => throw new InvalidOperationException()
                });
        }

        _handler.ReceivedWithAnyArgs(6).ChatCompletion(default, null);
        _messageParser.ReceivedWithAnyArgs(4).Parse(null!, null!);
    }

    [Fact]
    public void RunMultiStepInstructionShouldProcessSingleThreadCorrectly()
    {
        // Arrange
        var initialMessages = ImmutableList.Create(new Message(SenderRole.User, "Initial"));
        var workspace = new ChatWorkspace([CreateChatThread(initialMessages)]);
        var multiStepInstruction = Instruction.MultiStepInstruction.Of(
            Preamble: "Preamble",
            Messages: ["Step1", "Step2"],
            Ending: "Ending");

        // Act
        var result = workspace.RunInstruction(multiStepInstruction, _handler);

        // Assert
        result.Should().BeOk().ChatThreads.Should().HaveCount(2);
        result.Unwrap().ChatThreads[0].Messages.Should().HaveCount(3)
            .And.SatisfyRespectively(
                m => m.Content.Should().Be("Initial"),
                m => m.Content.Should().Be("PreambleStep1Ending"),
                m => m.Content.Should().Be("Response"));
        result.Unwrap().ChatThreads[1].Messages.Should().HaveCount(3)
            .And.SatisfyRespectively(
                m => m.Content.Should().Be("Initial"),
                m => m.Content.Should().Be("PreambleStep2Ending"),
                m => m.Content.Should().Be("Response"));
        _handler.ReceivedWithAnyArgs(2).ChatCompletion(default, null);
        _messageParser.DidNotReceiveWithAnyArgs().Parse(null!, null!);
    }

    [Fact]
    public void RunMultiStepInstructionShouldReturnErrorWhenMoreThanOneThreadExists()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread(), CreateChatThread()]);
        var multiStepInstruction = Instruction.MultiStepInstruction.Of(
            Preamble: "Preamble",
            Messages: ["Step1", "Step2"],
            Ending: "Ending");

        // Act
        var result = workspace.RunInstruction(multiStepInstruction, _handler);

        // Assert
        result.Should().BeErr().Should().Be("Only one multi-step instruction is supported.");
        _handler.DidNotReceiveWithAnyArgs().ChatCompletion(default, null);
        _messageParser.DidNotReceiveWithAnyArgs().Parse(null!, null!);
    }

    [Fact]
    public void RunInstructionShouldHandleEmptyWorkspace()
    {
        // Arrange
        var workspace = new ChatWorkspace([]);
        var instructions = new[]
        {
            Instruction.StepInstruction.Of("Step"),
            Instruction.MultiStepInstruction.Of("Preamble", ["Step1"], "Ending"),
            Instruction.JsonConvertInstruction.Of("key", "JSON", "{}"),
            Instruction.CodeExtractInstruction.Of("key", "Code")
        };

        foreach (var instruction in instructions)
        {
            // Act
            var result = workspace.RunInstruction(instruction, _handler);

            // Assert
            result.Should().BeOk().ChatThreads.Should().BeEmpty();
        }

        _handler.DidNotReceiveWithAnyArgs().ChatCompletion(default, null);
        _messageParser.DidNotReceiveWithAnyArgs().Parse(null!, null!);
    }

    private ChatThread CreateChatThread(ImmutableList<Message>? messages = null) =>
        new(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: messages ?? [],
            MessageParser: _messageParser);
}
