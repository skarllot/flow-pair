using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Models;

[TestSubject(typeof(ChatWorkspace))]
public class ChatWorkspaceTest
{
    private readonly ProgressTask _progressTask = new(0, "description", 100);
    private readonly IProxyCompleteChatHandler _handler = Substitute.For<IProxyCompleteChatHandler>();

    public ChatWorkspaceTest()
    {
        _handler
            .ChatCompletion(AllowedModel.Gpt4, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(Role.Assistant, "Response"));
    }

    [Fact]
    public void RunStepInstructionShouldUpdateSingleThread()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread()]);
        var stepInstruction = Instruction.StepInstruction.Of("Step Message");

        // Act
        var result = workspace.RunInstruction(stepInstruction, _handler);

        // Assert
        result.Should().BeOk()
            .ChatThreads.Should().HaveCount(1)
            .And.Subject.Should().ContainSingle(t => t.Messages.Count == 2);
        _progressTask.Value.Should().Be(1);
        _handler.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunStepInstructionShouldUpdateAllThreads()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread(), CreateChatThread()]);
        var stepInstruction = Instruction.StepInstruction.Of("Step Message");

        // Act
        var result = workspace.RunInstruction(stepInstruction, _handler);

        // Assert
        result.Should().BeOk()
            .ChatThreads.Should().HaveCount(2)
            .And.Subject.Should().OnlyContain(t => t.Messages.Count == 2);
        _progressTask.Value.Should().Be(2);
        _handler.ReceivedCalls().Should().HaveCount(2);
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
        result.Should().BeErr("Only one multi-step instruction is supported.");
        _handler.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void RunMultiStepInstructionShouldProcessSingleThreadCorrectly()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread()]);
        var multiStepInstruction = Instruction.MultiStepInstruction.Of(
            Preamble: "Preamble",
            Messages: ["Step1", "Step2"],
            Ending: "Ending");

        // Act
        var result = workspace.RunInstruction(multiStepInstruction, _handler);

        // Assert
        result.Should().BeOk()
            .ChatThreads.Should().HaveCount(2)
            .And.Subject.Should().OnlyContain(t => t.Messages.Count == 2);
        _handler.ReceivedCalls().Should().HaveCount(2);
    }

    [Fact]
    public void RunJsonInstructionShouldUpdateSingleThread()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread()]);
        var jsonInstruction = new Instruction.JsonConvertInstruction(
            "JSON Message",
            "{ \"schema\": \"value\" }");

        // Act
        var result = workspace.RunInstruction(jsonInstruction, _handler);

        // Assert
        result.Should().BeOk()
            .ChatThreads.Should().HaveCount(1)
            .And.Subject.Should().ContainSingle(t => t.Messages.Count == 2);
        _handler.ReceivedCalls().Should().HaveCount(1);
    }

    [Fact]
    public void RunJsonInstructionShouldUpdateAllThreads()
    {
        // Arrange
        var workspace = new ChatWorkspace([CreateChatThread(), CreateChatThread()]);
        var jsonInstruction = new Instruction.JsonConvertInstruction(
            "JSON Message",
            "{ \"schema\": \"value\" }");

        // Act
        var result = workspace.RunInstruction(jsonInstruction, _handler);

        // Assert
        result.Should().BeOk()
            .ChatThreads.Should().HaveCount(2)
            .And.Subject.Should().OnlyContain(t => t.Messages.Count == 2);
        _handler.ReceivedCalls().Should().HaveCount(2);
    }

    private ChatThread CreateChatThread(ImmutableList<Message>? messages = null) =>
        new(
            Progress: _progressTask,
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            ValidateJson: _ => Unit(),
            Messages: messages ?? []);
}
