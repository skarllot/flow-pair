using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Models;

[TestSubject(typeof(ChatScript))]
public class ChatScriptTest
{
    [Fact]
    public void FindChatScriptForFileShouldReturnCorrectScriptWhenMatchExists()
    {
        // Arrange
        List<ChatScript> scripts =
        [
            new(
                Name: "Script1",
                Extensions: [".txt", ".log"],
                SystemInstruction: "Instruction1",
                Instructions: []),
            new(
                Name: "Script2",
                Extensions: [".cs", ".java"],
                SystemInstruction: "Instruction2",
                Instructions: []),
        ];

        // Act
        var result = ChatScript.FindChatScriptForFile(scripts, "example.cs");

        // Assert
        result.Should().BeSome()
            .Name.Should().Be("Script2");
    }

    [Fact]
    public void FindChatScriptForFileShouldReturnNoneWhenNoMatchExists()
    {
        // Arrange
        List<ChatScript> scripts =
        [
            new(
                Name: "Script1",
                Extensions: [".txt", ".log"],
                SystemInstruction: "Instruction1",
                Instructions: ImmutableList<Instruction>.Empty),
            new(
                Name: "Script2",
                Extensions: [".cs", ".java"],
                SystemInstruction: "Instruction2",
                Instructions: ImmutableList<Instruction>.Empty),
        ];

        // Act
        var result = ChatScript.FindChatScriptForFile(scripts, "example.py");

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void TotalStepsShouldCalculateCorrectlyWithSingleStepInstructions()
    {
        // Arrange
        var script = new ChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "SystemInstruction",
            Instructions:
            [
                new Instruction.StepInstruction("Message1"),
                new Instruction.StepInstruction("Message2"),
            ]);

        // Act
        var totalSteps = script.TotalSteps;

        // Assert
        totalSteps.Should().Be(2);
    }

    [Fact]
    public void TotalStepsShouldCalculateCorrectlyWithMultiStepInstructions()
    {
        // Arrange
        var script = new ChatScript(
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
        var totalSteps = script.TotalSteps;

        // Assert
        totalSteps.Should().Be(4);
    }

    [Fact]
    public void TotalStepsShouldCalculateCorrectlyWithSingleAfterMultiStepInstructions()
    {
        // Arrange
        var script = new ChatScript(
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
                new Instruction.StepInstruction("Message2"),
            ]);

        // Act
        var totalSteps = script.TotalSteps;

        // Assert
        totalSteps.Should().Be(7);
    }
}
