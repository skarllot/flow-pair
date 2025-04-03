using System.Collections.Immutable;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Services;

[TestSubject(typeof(ReviewFeedbackBuilder))]
public sealed class ReviewFeedbackBuilderTest : IDisposable
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly TestConsole _console = new();
    private readonly IReviewChatScript _chatScript = Substitute.For<IReviewChatScript>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();

    private ReviewFeedbackBuilder CreateBuilder() =>
        new(
            timeProvider: _timeProvider,
            console: _console,
            chatScript: _chatScript,
            chatService: _chatService,
            tempFileWriter: _tempFileWriter);

    public void Dispose() => _console.Dispose();

    [Fact]
    public void RunShouldHandleMultipleFileChangesAndFeedback()
    {
        // Arrange
        var feedbackBuilder = CreateBuilder();

        _timeProvider.GetUtcNow()
            .Returns(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc));

        _chatScript
            .GetInitialMessages(
                Arg.Is<ReviewChangesRequest>(
                    r => r.Diff.Contains("diff content 1") &&
                         r.Diff.Contains("diff content 2")))
            .Returns([new Message(SenderRole.User, "diff content")]);
        _chatScript.Extensions
            .Returns([".cs"]);
        _chatScript.Instructions
            .Returns([Instruction.StepInstruction.Of("Review changes")]);

        var feedbackResponses = ImmutableList.Create(
            new ReviewerFeedbackResponse(
                RiskScore: 1,
                RiskDescription: "Low risk",
                Title: "Feedback 1",
                Category: "Category 1",
                Language: "C#",
                Feedback: "Feedback content 1",
                Path: "path/to/file1.cs",
                LineRange: "1-10"),
            new ReviewerFeedbackResponse(
                RiskScore: 2,
                RiskDescription: "Medium risk",
                Title: "Feedback 2",
                Category: "Category 2",
                Language: "C#",
                Feedback: "Feedback content 2",
                Path: "path/to/file2.cs",
                LineRange: "5-15"));

        _chatService
            .Run(
                input: Arg.Any<ReviewChangesRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatScript: Arg.Any<IProcessableChatScript<
                    ReviewChangesRequest,
                    ImmutableList<ReviewerFeedbackResponse>>>())
            .Returns(feedbackResponses);

        // Act
        var fileChanges = ImmutableList.Create(
            new FileChange("path/to/file1.cs", "diff content 1"),
            new FileChange("path/to/file2.cs", "diff content 2"));
        var result = feedbackBuilder.Run(fileChanges);

        // Assert
        result.Should().BeSome();
        _tempFileWriter.Received(1).Write(
            Arg.Is<string>(s => s == "20250102030405-feedback.html"),
            Arg.Is<string>(s => s.Contains("Feedback 1") && s.Contains("Feedback 2")));
        _console.Output.Should().Contain("Created 2 comments");
    }

    [Fact]
    public void RunShouldHandleErrorsInChatServiceGracefully()
    {
        // Arrange
        var feedbackBuilder = CreateBuilder();

        _chatScript.Extensions
            .Returns([".cs"]);
        _chatScript.Instructions
            .Returns([Instruction.StepInstruction.Of("Review changes")]);

        _chatService
            .Run(
                input: Arg.Any<ReviewChangesRequest>(),
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatScript: Arg.Any<IProcessableChatScript<
                    ReviewChangesRequest,
                    ImmutableList<ReviewerFeedbackResponse>>>())
            .Returns("Error in Chat Service");

        // Act
        var fileChanges = ImmutableList.Create(new FileChange("path/to/file.cs", "diff content"));
        var result = feedbackBuilder.Run(fileChanges);

        // Assert
        result.Should().BeSome();
        _console.Output.Should()
            .Contain("Error: Error in Chat Service")
            .And.Contain("Created 0 comments");
        _tempFileWriter.DidNotReceiveWithAnyArgs().Write(null!, null!);
    }

    [Fact]
    public void ExecuteShouldIgnoreFilesThatDontMatchChatScript()
    {
        // Arrange
        var feedbackBuilder = CreateBuilder();

        _chatScript
            .GetInitialMessages(
                Arg.Is<ReviewChangesRequest>(
                    r => r.Diff.Contains("diff content cs") &&
                         !r.Diff.Contains("diff content js")))
            .Returns([new Message(SenderRole.User, "diff content")]);
        _chatScript.Extensions
            .Returns([".cs"]);
        _chatScript.Instructions
            .Returns([Instruction.StepInstruction.Of("Review changes")]);

        _chatService
            .Run(
                input: Arg.Is<ReviewChangesRequest>(
                    r => r.Diff.Contains("diff content cs") &&
                         !r.Diff.Contains("diff content js")),
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatScript: Arg.Any<IProcessableChatScript<
                    ReviewChangesRequest,
                    ImmutableList<ReviewerFeedbackResponse>>>())
            .Returns(ImmutableList<ReviewerFeedbackResponse>.Empty);

        // Act
        var fileChanges = ImmutableList.Create(
            new FileChange("path/to/file.cs", "diff content cs"),
            new FileChange("path/to/file.js", "diff content js"));
        var result = feedbackBuilder.Run(fileChanges);

        // Assert
        result.Should().BeSome();
        _console.Output.Should().Contain("Created 0 comments");
        _tempFileWriter.DidNotReceiveWithAnyArgs().Write(null!, null!);
    }
}
