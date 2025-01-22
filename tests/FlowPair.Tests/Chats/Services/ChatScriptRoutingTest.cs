using System.Collections.Immutable;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(ChatScriptRouting))]
public class ChatScriptRoutingTest
{
    [Fact]
    public void FindChatScriptForFileShouldReturnCorrectScriptWhenMatchExists()
    {
        // Arrange
        List<TestChatScript> scripts =
        [
            new(
                Name: "Script1",
                Extensions: [".txt", ".log"],
                SystemInstruction: "Instruction1",
                InitialMessages: [],
                Instructions: []),
            new(
                Name: "Script2",
                Extensions: [".cs", ".java"],
                SystemInstruction: "Instruction2",
                InitialMessages: [],
                Instructions: []),
        ];

        // Act
        var result = ChatScriptRouting.FindChatScriptForFile(scripts, "example.cs");

        // Assert
        result.Should().BeSome()
            .Name.Should().Be("Script2");
    }

    [Fact]
    public void FindChatScriptForFileShouldReturnNoneWhenNoMatchExists()
    {
        // Arrange
        List<TestChatScript> scripts =
        [
            new(
                Name: "Script1",
                Extensions: [".txt", ".log"],
                SystemInstruction: "Instruction1",
                InitialMessages: [],
                Instructions: []),
            new(
                Name: "Script2",
                Extensions: [".cs", ".java"],
                SystemInstruction: "Instruction2",
                InitialMessages: [],
                Instructions: []),
        ];

        // Act
        var result = ChatScriptRouting.FindChatScriptForFile(scripts, "example.py");

        // Assert
        result.Should().BeNone();
    }

    private sealed record TestChatScript(
        string Name,
        ImmutableArray<string> Extensions,
        string SystemInstruction,
        ImmutableList<Message> InitialMessages,
        ImmutableList<Instruction> Instructions) : IChatScript;
}
