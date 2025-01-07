using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Chats.Services;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Tests.Chats.Models;

[TestSubject(typeof(ChatThread))]
public class ChatThreadTest
{
    private const string CompletionResponse = "Response";
    private readonly ProgressTask _progressTask = new(0, "description", 100);
    private readonly IProxyCompleteChatHandler _handler = Substitute.For<IProxyCompleteChatHandler>();
    private readonly IMessageParser _messageParser = Substitute.For<IMessageParser>();

    public ChatThreadTest()
    {
        _handler
            .ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, CompletionResponse));
    }

    [Fact]
    public void RunStepInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

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
        _messageParser.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void RunMultiStepInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

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
        _messageParser.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void RunJsonInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        const string outputKey = "TestKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns(Unit());

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

        var jsonInstruction = new Instruction.JsonConvertInstruction(
            OutputKey: outputKey,
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
        _messageParser.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunJsonInstructionShouldRetryAddMessagesAndIncrementProgress()
    {
        // Arrange
        const string outputKey = "TestKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns("First try", "Second try", Unit());

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

        var jsonInstruction = new Instruction.JsonConvertInstruction(
            OutputKey: outputKey,
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
        _messageParser.ReceivedCalls().Should().HaveCount(3);
    }

    [Fact]
    public void RunStepInstructionShouldNotAddMessageWhenInterrupted()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.Assistant, "Interrupted <STOP>")],
            MessageParser: _messageParser);

        var stepInstruction = new Instruction.StepInstruction("New Message");

        // Act
        var result = chatThread.RunStepInstruction(stepInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(1);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().BeEmpty();
        _messageParser.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void RunCodeInstructionShouldAddMessageAndIncrementProgress()
    {
        // Arrange
        const string outputKey = "CodeKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns(Unit());

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

        var codeInstruction = new Instruction.CodeExtractInstruction(
            OutputKey: outputKey,
            Message: "Extract Code");

        // Act
        var result = chatThread.RunCodeInstruction(codeInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(3);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(1);
        _messageParser.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunCodeInstructionShouldRetryAddMessagesAndIncrementProgress()
    {
        // Arrange
        const string outputKey = "CodeKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns("First try", "Second try", Unit());

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

        var codeInstruction = new Instruction.CodeExtractInstruction(
            OutputKey: outputKey,
            Message: "Extract Code");

        // Act
        var result = chatThread.RunCodeInstruction(codeInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(7);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(3);
        _messageParser.ReceivedCalls().Should().HaveCount(3);
    }

    [Fact]
    public void RunCodeInstructionShouldNotAddMessageWhenInterrupted()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: "<STOP>",
            Messages: [new Message(SenderRole.Assistant, "Interrupted <STOP>")],
            MessageParser: _messageParser);

        var codeInstruction = new Instruction.CodeExtractInstruction(
            OutputKey: "CodeKey",
            Message: "Extract Code");

        // Act
        var result = chatThread.RunCodeInstruction(codeInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(1);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().BeEmpty();
        _messageParser.ReceivedCalls().Should().BeEmpty();
    }
}
