using System.Collections.Immutable;
using System.Text.Json;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Services;

[TestSubject(typeof(ChatService))]
public class ChatServiceTest
{
    private readonly IProxyCompleteChatHandler _completeChatHandler = Substitute.For<IProxyCompleteChatHandler>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();
    private readonly ChatService _chatService;
    private readonly Progress _progress = new(AnsiConsole.Create(new AnsiConsoleSettings()));

    public ChatServiceTest()
    {
        _chatService = new ChatService(AgentJsonContext.Default, _completeChatHandler, _tempFileWriter);
    }

    [Fact]
    public void RunMultipleShouldReturnValidFeedbackWhenChatScriptIsValid()
    {
        // Arrange
        var chatScript = new ChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "System Instruction",
            Instructions: [new Instruction.StepInstruction("Step Message")]);

        var feedbackResponses = ImmutableList.Create(
            new ReviewerFeedbackResponse(
                RiskScore: 10,
                RiskDescription: "High Risk",
                Title: "Title",
                Category: "Category",
                Language: "Language",
                Feedback: "Feedback",
                Path: "Path",
                LineRange: "LineRange"));

        _completeChatHandler
            .ChatCompletion(AllowedModel.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(
                new Message(
                    Role.Assistant,
                    $"""
                     ```json
                     {JsonSerializer.Serialize(feedbackResponses, AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse)}
                     ```
                     """));

        // Act
        var result = _chatService.RunMultiple(
            progress: _progress,
            model: AllowedModel.Claude35Sonnet,
            chatScript: chatScript,
            initialMessages: [new Message(Role.User, "Initial Content")],
            jsonTypeInfo: AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse);

        // Assert
        result.Should().BeOk()
            .Should().BeEquivalentTo(feedbackResponses);
    }

    [Fact]
    public void RunMultipleShouldReturnErrorWhenDeserializationFails()
    {
        // Arrange
        var chatScript = new ChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "System Instruction",
            Instructions: [new Instruction.StepInstruction("Step Message")]);

        _completeChatHandler
            .ChatCompletion(AllowedModel.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(Role.Assistant, "Invalid Feedback Content"));

        // Act
        var result = _chatService.RunMultiple(
            progress: _progress,
            model: AllowedModel.Claude35Sonnet,
            chatScript: chatScript,
            initialMessages: [new Message(Role.User, "Initial Content")],
            jsonTypeInfo: AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse);

        // Assert
        result.Should().BeOk()
            .Should().BeEmpty();
    }

    [Fact]
    public void RunMultipleShouldSaveChatHistoryWhenExecutionSucceeds()
    {
        // Arrange
        var chatScript = new ChatScript(
            Name: "TestScript",
            Extensions: [".txt"],
            SystemInstruction: "System Instruction",
            Instructions: [new Instruction.StepInstruction("Step Message")]);

        var initialMessages = new[] { new Message(Role.User, "Initial Content") };

        var feedbackResponses = ImmutableList.Create(
            new ReviewerFeedbackResponse(
                10,
                "High Risk",
                "Title",
                "Category",
                "Language",
                "Feedback",
                "Path",
                "LineRange"));

        _completeChatHandler
            .ChatCompletion(AllowedModel.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(Role.Assistant, "Feedback Content"));

        // Act
        _chatService.RunMultiple(
            progress: _progress,
            model: AllowedModel.Claude35Sonnet,
            chatScript: chatScript,
            initialMessages: initialMessages,
            jsonTypeInfo: AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse);

        // Assert
        _tempFileWriter.WriteJson(
            Arg.Any<string>(),
            Arg.Any<ImmutableList<ImmutableList<Message>>>(),
            AgentJsonContext.Default.ImmutableListImmutableListMessage);
    }
}
