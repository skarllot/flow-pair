using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.ReviewChanges;

[TestSubject(typeof(ReviewChangesCommand))]
public class ReviewChangesCommandTest
{
    private readonly IAnsiConsole _console = Substitute.For<IAnsiConsole>();
    private readonly IGitGetChangesHandler _getChangesHandler = Substitute.For<IGitGetChangesHandler>();
    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IChatService _chatService = Substitute.For<IChatService>();
    private readonly ITempFileWriter _tempFileWriter = Substitute.For<ITempFileWriter>();

    private ReviewChangesCommand CreateCommand() =>
        new(
            console: _console,
            chatDefinition: new ReviewChatDefinition(AgentJsonContext.Default),
            getChangesHandler: _getChangesHandler,
            loginUseCase: _loginUseCase,
            chatService: _chatService,
            tempFileWriter: _tempFileWriter);

    [Fact]
    public void ExecuteShouldReturnZeroWhenEverythingSucceeds()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(new FileChange("path/to/file", "diff content"));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase.Execute(isBackground: true).Returns(0);

        _chatService
            .Run(
                Arg.Any<Progress>(),
                LlmModelType.Claude35Sonnet,
                Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                Arg.Any<IReadOnlyList<Message>>())
            .Returns(ImmutableList<ReviewerFeedbackResponse>.Empty);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ExecuteShouldReturnErrorWhenLoginFails()
    {
        // Arrange
        var command = CreateCommand();

        _getChangesHandler.Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ImmutableList.Create<FileChange>());

        _loginUseCase.Execute(isBackground: true).Returns(1);

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BuildFeedbackShouldCreateFeedbackFileWhenFeedbackIsGenerated()
    {
        // Arrange
        var command = CreateCommand();

        _getChangesHandler
            .Extract("repo/path", "commitHash")
            .Returns(ImmutableList.Create(new FileChange("path/to/file.cs", "diff content")));

        var feedbackResponse = new ReviewerFeedbackResponse(
            RiskScore: 2,
            RiskDescription: "Medium priority adjustments",
            Title: "Refactor ProxyClient using factory pattern",
            Category: "Design Pattern",
            Language: "C#",
            Feedback: "The ProxyClient class has too many dependencies and responsibilities. " +
                      "Consider refactoring using a factory pattern to improve maintainability and scalability",
            Path: "/src/FlowReviewer/Flow/ProxyCompleteChat/ProxyClient.cs",
            LineRange: "13-49");

        _chatService
            .Run(
                Arg.Any<Progress>(),
                LlmModelType.Claude35Sonnet,
                Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                Arg.Any<IReadOnlyList<Message>>())
            .Returns(ImmutableList.Create(feedbackResponse));

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _tempFileWriter.Received().Write(Arg.Any<string>(), Arg.Is<string>(x => x.Contains(feedbackResponse.Title)));
    }

    [Fact]
    public void ExecuteShouldHandleErrorsInChatServiceGracefully()
    {
        // Arrange
        var command = CreateCommand();
        var fileChanges = ImmutableList.Create(new FileChange("path/to/file", "diff content"));

        _getChangesHandler
            .Extract(Arg.Any<string>(), Arg.Any<string>())
            .Returns(fileChanges);

        _loginUseCase.Execute(isBackground: true).Returns(0);

        _chatService
            .Run(
                Arg.Any<Progress>(),
                LlmModelType.Claude35Sonnet,
                Arg.Any<IChatDefinition<ImmutableList<ReviewerFeedbackResponse>>>(),
                Arg.Any<IReadOnlyList<Message>>())
            .Returns("Error in Chat Service");

        // Act
        var result = command.Execute("repo/path", "commitHash");

        // Assert
        result.Should().Be(0);
        _console.Received().Write(Arg.Any<IRenderable>());
    }
}
