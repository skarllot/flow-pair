﻿using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
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
    private readonly TestConsole _console = new();
    private readonly IReviewChatDefinition _chatDefinition = Substitute.For<IReviewChatDefinition>();
    private readonly IGitGetChangesHandler _getChangesHandler = Substitute.For<IGitGetChangesHandler>();
    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();

    private ReviewChangesCommand CreateCommand() =>
        new(
            console: _console,
            chatScript: _chatDefinition,
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
        _chatService.DidNotReceiveWithAnyArgs().Run<ReviewerFeedbackResponse>(null, default, null, null);
    }

    [Fact]
    public void ExecuteShouldHandleMultipleFileChangesAndFeedback()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(
            new FileChange("path/to/file1.cs", "diff content 1"),
            new FileChange("path/to/file2.cs", "diff content 2"));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase
            .Execute(isBackground: true)
            .Returns(0);

        _chatDefinition.ChatScript
            .Returns(new ChatScript("test", [".cs"], "system", [Instruction.StepInstruction.Of("Review changes")]));

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
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatDefinition: Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                initialMessages: Arg.Is<IReadOnlyList<Message>>(
                    m => m.Count == 1 && m[0].Content.Contains("diff content 1") &&
                         m[0].Content.Contains("diff content 2")))
            .Returns(feedbackResponses);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _tempFileWriter.Received(1).Write(
            Arg.Is<string>(s => s.EndsWith("-feedback.html")),
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

        _chatDefinition.ChatScript
            .Returns(new ChatScript("test", [".cs"], "system", [Instruction.StepInstruction.Of("Review changes")]));

        _chatService
            .Run(
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatDefinition: Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                initialMessages: Arg.Any<IReadOnlyList<Message>>())
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

        _chatDefinition.ChatScript
            .Returns(new ChatScript("test", [".cs"], "system", [Instruction.StepInstruction.Of("Review changes")]));

        _chatService
            .Run(
                progress: Arg.Any<Progress>(),
                llmModelType: LlmModelType.Claude35Sonnet,
                chatDefinition: Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                initialMessages: Arg.Is<IReadOnlyList<Message>>(m => m.Count == 1 && m[0].Content == "diff content cs"))
            .Returns(ImmutableList<ReviewerFeedbackResponse>.Empty);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _chatService.Received(1).Run(
            Arg.Any<Progress>(),
            LlmModelType.Claude35Sonnet,
            Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
            Arg.Is<IReadOnlyList<Message>>(m => m.Count == 1 && m[0].Content == "diff content cs"));
        _console.Output.Should().Contain("Created 0 comments");
        _tempFileWriter.DidNotReceiveWithAnyArgs().Write(null!, null!);
    }
}
