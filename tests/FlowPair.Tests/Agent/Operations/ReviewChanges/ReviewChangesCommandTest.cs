using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.ReviewChanges;

[TestSubject(typeof(ReviewChangesCommand))]
public sealed class ReviewChangesCommandTest : IDisposable
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly TestConsole _console = new();
    private readonly IReviewChatScript _chatScript = Substitute.For<IReviewChatScript>();
    private readonly IGitGetChangesHandler _getChangesHandler = Substitute.For<IGitGetChangesHandler>();
    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();

    private ReviewChangesCommand CreateCommand() =>
        new(
            timeProvider: _timeProvider,
            console: _console,
            chatScript: _chatScript,
            getChangesHandler: _getChangesHandler,
            loginUseCase: _loginUseCase,
            chatService: _chatService,
            tempFileWriter: _tempFileWriter);

    public void Dispose() => _console.Dispose();

    [Fact]
    public void ExecuteShouldNotFailWhenGetChangesHandlerFails()
    {
        // Arrange
        var command = CreateCommand();

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(None);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _loginUseCase.DidNotReceiveWithAnyArgs().Execute(false);
    }

    [Fact]
    public void ExecuteShouldReturnErrorWhenLoginFails()
    {
        // Arrange
        var command = CreateCommand();

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ImmutableList.Create<FileChange>());

        _loginUseCase
            .Execute(isBackground: true)
            .Returns(1);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(1);
        _chatService.DidNotReceiveWithAnyArgs()
            .Run<ReviewChangesRequest, ReviewerFeedbackResponse>(null!, null, 0, null);
    }

    [Fact]
    public void ExecuteShouldHandleMultipleFileChangesAndFeedback()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(
            new FileChange("path/to/file1.cs", "diff content 1"),
            new FileChange("path/to/file2.cs", "diff content 2"));

        _timeProvider.GetUtcNow()
            .Returns(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase
            .Execute(isBackground: true)
            .Returns(0);

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
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _tempFileWriter.Received(1).Write(
            Arg.Is<string>(s => s == "20250102030405-feedback.html"),
            Arg.Is<string>(s => s.Contains("Feedback 1") && s.Contains("Feedback 2")));
        _console.Output.Should().Contain("Created 2 comments");
    }

    [Fact]
    public void ExecuteShouldHandleErrorsInChatServiceGracefully()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(new FileChange("path/to/file.cs", "diff content"));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase
            .Execute(isBackground: true)
            .Returns(0);

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
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _console.Output.Should()
            .Contain("Error: Error in Chat Service")
            .And.Contain("Created 0 comments");
        _tempFileWriter.DidNotReceiveWithAnyArgs().Write(null!, null!);
    }

    [Fact]
    public void ExecuteShouldIgnoreFilesThatDontMatchChatScript()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(
            new FileChange("path/to/file.cs", "diff content cs"),
            new FileChange("path/to/file.js", "diff content js"));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase
            .Execute(isBackground: true)
            .Returns(0);

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
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _console.Output.Should().Contain("Created 0 comments");
        _tempFileWriter.DidNotReceiveWithAnyArgs().Write(null!, null!);
    }
}
