using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Models;

[TestSubject(typeof(ChatThread))]
public class ChatThreadTest
{
    private const string CompletionResponse = "Response";
    private const string StopKeyword = "<STOP>";
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
    public void ConstructorShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var initialMessages = ImmutableList.Create(new Message(SenderRole.User, "Initial"));
        var initialOutputs = ImmutableDictionary<string, object>.Empty.Add("Key", "Value");

        // Act
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
            Messages: initialMessages,
            MessageParser: _messageParser,
            Outputs: initialOutputs);

        // Assert
        chatThread.Progress.Should().BeSameAs(_progressTask);
        chatThread.ModelType.Should().Be(LlmModelType.Gpt4);
        chatThread.Messages.Should().BeEquivalentTo(initialMessages);
        chatThread.Outputs.Should().BeEquivalentTo(initialOutputs);
        chatThread.LastMessage.Should().BeEquivalentTo(initialMessages[0]);
    }

    [Fact]
    public void ConstructorShouldInitializeOutputsWhenNull()
    {
        // Arrange & Act
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
            Messages: ImmutableList<Message>.Empty,
            MessageParser: _messageParser,
            Outputs: null);

        // Assert
        chatThread.Outputs.Should().NotBeNull().And.BeEmpty();
        chatThread.LastMessage.Should().BeNull();
    }

    [Fact]
    public void RunStepInstructionShouldAddMessageAndCompleteChat()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
            Messages: [new Message(SenderRole.User, "Initial")],
            MessageParser: _messageParser);

        var stepInstruction = new Instruction.StepInstruction("Step Message");

        // Act
        var result = chatThread.RunStepInstruction(stepInstruction, _handler);

        // Assert
        result.IsOk.Should().BeTrue();
        var updatedThread = result.Unwrap();
        updatedThread.Messages.Should().HaveCount(3);
        updatedThread.Messages[1].Role.Should().Be(SenderRole.User);
        updatedThread.Messages[1].Content.Should().Contain("Step Message");
        updatedThread.LastMessage.Should().NotBeNull();
        updatedThread.LastMessage!.Role.Should().Be(SenderRole.Assistant);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.Received(1).ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>());
    }

    [Fact]
    public void RunMultiStepInstructionShouldAddMessageWithCorrectContent()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
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
        updatedThread.Messages[1].Role.Should().Be(SenderRole.User);
        updatedThread.Messages[1].Content.Should().Contain("Preamble")
            .And.Contain("Step2")
            .And.Contain("Ending");
        updatedThread.LastMessage.Should().NotBeNull();
        updatedThread.LastMessage!.Role.Should().Be(SenderRole.Assistant);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        _progressTask.Value.Should().Be(1);
        _handler.Received(1).ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>());
    }

    [Fact]
    public void RunJsonInstructionShouldAddMessageAndParseOutput()
    {
        // Arrange
        const string outputKey = "TestKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns(new { Value = "Parsed" });

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
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
        updatedThread.Messages[1].Role.Should().Be(SenderRole.User);
        updatedThread.Messages[1].Content.Should().Contain("JSON Message")
            .And.Contain("{ \"schema\": \"value\" }");
        updatedThread.LastMessage.Should().NotBeNull();
        updatedThread.LastMessage!.Role.Should().Be(SenderRole.Assistant);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        updatedThread.Outputs.Should().ContainKey(outputKey);
        updatedThread.Outputs[outputKey].Should().BeEquivalentTo(new { Value = "Parsed" });
        _progressTask.Value.Should().Be(1);
        _handler.Received(1).ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>());
        _messageParser.Received(1).Parse(outputKey, CompletionResponse);
    }

    [Fact]
    public void RunCodeInstructionShouldAddMessageAndParseOutput()
    {
        // Arrange
        const string outputKey = "CodeKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns(Ok<object, string>("Parsed Code"));

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
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
        updatedThread.Messages[1].Role.Should().Be(SenderRole.User);
        updatedThread.Messages[1].Content.Should().Contain("Extract Code");
        updatedThread.LastMessage.Should().NotBeNull();
        updatedThread.LastMessage!.Role.Should().Be(SenderRole.Assistant);
        updatedThread.LastMessage!.Content.Should().Be(CompletionResponse);
        updatedThread.Outputs.Should().ContainKey(outputKey);
        updatedThread.Outputs[outputKey].Should().Be("Parsed Code");
        _progressTask.Value.Should().Be(1);
        _handler.Received(1).ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>());
        _messageParser.Received(1).Parse(outputKey, CompletionResponse);
    }

    [Fact]
    public void RunJsonInstructionShouldRetryOnParseFailure()
    {
        // Arrange
        const string outputKey = "TestKey";
        _messageParser
            .Parse(outputKey, Arg.Any<string>())
            .Returns("Error", "Error", new { Value = "Parsed" });

        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
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
        updatedThread.Messages.Should().HaveCount(7); // Initial + 3 attempts * 2 messages each
        updatedThread.Outputs.Should().ContainKey(outputKey);
        updatedThread.Outputs[outputKey].Should().BeEquivalentTo(new { Value = "Parsed" });
        _progressTask.Value.Should().Be(1);
        _handler.Received(3).ChatCompletion(LlmModelType.Gpt4, Arg.Any<ImmutableList<Message>>());
        _messageParser.Received(3).Parse(outputKey, CompletionResponse);
    }

    [Fact]
    public void AllInstructionsShouldNotAddMessageWhenInterrupted()
    {
        // Arrange
        var chatThread = new ChatThread(
            Progress: _progressTask,
            ModelType: LlmModelType.Gpt4,
            StopKeyword: StopKeyword,
            Messages: [new Message(SenderRole.Assistant, $"Interrupted {StopKeyword}")],
            MessageParser: _messageParser);

        var stepInstruction = new Instruction.StepInstruction("Step");
        var multiStepInstruction = new Instruction.MultiStepInstruction("Pre", ["Step1"], "Post");
        var jsonInstruction = new Instruction.JsonConvertInstruction("Key", "JSON", "{}");
        var codeInstruction = new Instruction.CodeExtractInstruction("Key", "Code");

        // Act & Assert
        chatThread.RunStepInstruction(stepInstruction, _handler).Unwrap().Messages.Should().HaveCount(1);
        chatThread.RunMultiStepInstruction(multiStepInstruction, 0, _handler).Unwrap().Messages.Should().HaveCount(1);
        chatThread.RunJsonInstruction(jsonInstruction, _handler).Unwrap().Messages.Should().HaveCount(1);
        chatThread.RunCodeInstruction(codeInstruction, _handler).Unwrap().Messages.Should().HaveCount(1);

        _progressTask.Value.Should().Be(4); // Step, MultiStep, Json, and Code instructions increment progress
        _handler.DidNotReceiveWithAnyArgs().ChatCompletion(default, null!);
        _messageParser.DidNotReceiveWithAnyArgs().Parse(null!, null!);
    }
}
