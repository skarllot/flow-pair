using System.Collections.Immutable;
using System.Text.Json;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Infrastructure;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Flow.Operations.ProxyCompleteChat;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(ChatService))]
public class ChatServiceTest
{
    private readonly IProxyCompleteChatHandler _completeChatHandler = Substitute.For<IProxyCompleteChatHandler>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();
    private readonly ChatService _chatService;

    private readonly IChatDefinition<ImmutableList<ReviewerFeedbackResponse>> _chatDefinition =
        Substitute.For<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>();

    private readonly Progress _progress = new(AnsiConsole.Create(new AnsiConsoleSettings()));

    public ChatServiceTest()
    {
        _chatService = new ChatService(ChatJsonContext.Default, _completeChatHandler, _tempFileWriter);
    }

    [Fact]
    public void RunShouldReturnValidFeedbackWhenChatScriptIsValid()
    {
        // Arrange
        const string outputKey = "TestKey";
        _chatDefinition.ChatScript.Returns(
            new ChatScript(
                Name: "TestScript",
                Extensions: [".txt"],
                SystemInstruction: "System Instruction",
                Instructions: [new Instruction.JsonConvertInstruction(outputKey, "Step Message", "{}")]));
        _chatDefinition
            .Parse(outputKey, Arg.Any<string>())
            .Returns(
                c => JsonContentDeserializer
                    .TryDeserialize((string)c[1], AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse)
                    .Select(static object (x) => x));
        _chatDefinition
            .ConvertResult(Arg.Any<ChatWorkspace>())
            .Returns(c => OutputProcessor.AggregateLists<ReviewerFeedbackResponse>(c.Arg<ChatWorkspace>(), outputKey));

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
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(
                new Message(
                    SenderRole.Assistant,
                    $"""
                     ```json
                     {JsonSerializer.Serialize(
                         feedbackResponses,
                         AgentJsonContext.Default.ImmutableListReviewerFeedbackResponse)}
                     ```
                     """));

        // Act
        var result = _chatService.Run(
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatDefinition: _chatDefinition,
            initialMessages: [new Message(SenderRole.User, "Initial Content")]);

        // Assert
        result.Should().BeOk()
            .Should().BeEquivalentTo(feedbackResponses);
    }

    [Fact]
    public void RunShouldReturnErrorWhenDeserializationFails()
    {
        // Arrange
        _chatDefinition.ChatScript.Returns(
            new ChatScript(
                Name: "TestScript",
                Extensions: [".txt"],
                SystemInstruction: "System Instruction",
                Instructions: [new Instruction.StepInstruction("Step Message")]));

        _completeChatHandler
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "Invalid Feedback Content"));

        // Act
        var result = _chatService.Run(
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatDefinition: _chatDefinition,
            initialMessages: [new Message(SenderRole.User, "Initial Content")]);

        // Assert
        result.Should().BeErr("Failed to produce a valid output content");
    }

    [Fact]
    public void RunShouldSaveChatHistoryWhenExecutionSucceeds()
    {
        // Arrange
        _chatDefinition.ChatScript.Returns(
            new ChatScript(
                Name: "TestScript",
                Extensions: [".txt"],
                SystemInstruction: "System Instruction",
                Instructions: [new Instruction.StepInstruction("Step Message")]));

        var initialMessages = new[] { new Message(SenderRole.User, "Initial Content") };

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
            .ChatCompletion(LlmModelType.Claude35Sonnet, Arg.Any<ImmutableList<Message>>())
            .Returns(new Message(SenderRole.Assistant, "Feedback Content"));

        // Act
        _chatService.Run(
            progress: _progress,
            llmModelType: LlmModelType.Claude35Sonnet,
            chatDefinition: _chatDefinition,
            initialMessages: initialMessages);

        // Assert
        _tempFileWriter.Received(1).WriteJson(
            Arg.Any<string>(),
            Arg.Any<ImmutableList<ImmutableList<Message>>>(),
            ChatJsonContext.Default.ImmutableListImmutableListMessage);
    }
}
