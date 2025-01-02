using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Models;

[TestSubject(typeof(ChatThread))]
public class ChatThreadTest
{
    private const string CompletionResponse = "Response";
    private readonly ProgressTask _progressTask = new(0, "description", 100);
    private readonly IProxyCompleteChatHandler _handler = Substitute.For<IProxyCompleteChatHandler>();

    public ChatThreadTest()
    {
        _handler
            .ChatCompletion(AllowedModel.Gpt4, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(Role.Assistant, CompletionResponse));
    }

    [Fact]
    public void RunStepInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: _ => Unit(),
            Messages: [new Message(Role.User, "Initial")]);

        var stepInstruction = new Instruction.StepInstruction("New Message");

        // Act
        var result = chatThread.RunStepInstruction(stepInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(3);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunMultiStepInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: _ => Unit(),
            Messages: [new Message(Role.User, "Initial")]);

        var multiStepInstruction = new Instruction.MultiStepInstruction(
            Preamble: "Preamble",
            Messages: ["Step1", "Step2"],
            Ending: "Ending");

        // Act
        var result = chatThread.RunMultiStepInstruction(multiStepInstruction, 1, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(3);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunJsonInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: _ => Unit(),
            Messages: [new Message(Role.User, "Initial")]);

        var jsonInstruction = new Instruction.JsonConvertInstruction(
            Message: "JSON Message",
            JsonSchema: "{ \"schema\": \"value\" }");

        // Act
        var result = chatThread.RunJsonInstruction(jsonInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(3);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunJsonInstructionShouldRetryAddMessagesAndIncrementProgress()
    {
        // Arrange
        var validateCallCount = 0;
        Func<ReadOnlySpan<char>, Result<Unit, string>> validateJson = _ => validateCallCount++ switch
        {
            0 => "First try",
            1 => "Second try",
            _ => Unit(),
        };

        var chatThread = new ChatThread(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: validateJson,
            Messages: [new Message(Role.User, "Initial")]);

        var jsonInstruction = new Instruction.JsonConvertInstruction(
            Message: "JSON Message",
            JsonSchema: "{ \"schema\": \"value\" }");

        // Act
        var result = chatThread.RunJsonInstruction(jsonInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(7);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(3);
    }

    [Fact]
    public void RunStepInstructionShouldNotAddMessageWhenInterrupted()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: _ => Unit(),
            Messages: [new Message(Role.Assistant, "Interrupted <STOP>")]);

        var stepInstruction = new Instruction.StepInstruction("New Message");

        // Act
        var result = chatThread.RunStepInstruction(stepInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(1);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().BeEmpty();
    }
}
