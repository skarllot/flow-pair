using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Contracts.v1;

[TestSubject(typeof(ChatScriptExtensions))]
public class ChatScriptExtensionsTest
{
    [Fact]
    public void GetTotalStepsShouldCalculateCorrectlyWithSingleStepInstructions()
    {
        // Arrange
        var script = new TestChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "SystemInstruction",
            Instructions:
            [
                new Instruction.StepInstruction("Message1"),
                new Instruction.StepInstruction("Message2"),
            ]);

        // Act
        var totalSteps = script.GetTotalSteps();

        // Assert
        totalSteps.Should().Be(2);
    }

    [Fact]
    public void GetTotalStepsShouldCalculateCorrectlyWithMultiStepInstructions()
    {
        // Arrange
        var script = new TestChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "SystemInstruction",
            Instructions:
            [
                new Instruction.StepInstruction("Message1"),
                new Instruction.MultiStepInstruction(
                    Preamble: "Preamble",
                    Messages: ImmutableList.Create("Step1", "Step2", "Step3"),
                    Ending: "Ending"),
            ]);

        // Act
        var totalSteps = script.GetTotalSteps();

        // Assert
        totalSteps.Should().Be(4);
    }

    [Fact]
    public void GetTotalStepsShouldCalculateCorrectlyWithSingleAfterMultiStepInstructions()
    {
        // Arrange
        var script = new TestChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "SystemInstruction",
            Instructions:
            [
                new Instruction.StepInstruction("Message1"),
                new Instruction.MultiStepInstruction(
                    Preamble: "Preamble",
                    Messages: ImmutableList.Create("Step1", "Step2", "Step3"),
                    Ending: "Ending"),
                new Instruction.CodeExtractInstruction("Key1", "Message2"),
            ]);

        // Act
        var totalSteps = script.GetTotalSteps();

        // Assert
        totalSteps.Should().Be(7);
    }

    private sealed record TestChatScript(
        string Name,
        ImmutableArray<string> Extensions,
        string SystemInstruction,
        ImmutableList<Instruction> Instructions) : IChatScript;
}
