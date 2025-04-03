using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewPullRequest;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;

namespace Raiqub.LlmTools.FlowPair.Tests.Agent.Operations.ReviewPullRequest;

[TestSubject(typeof(ReviewPullRequestCommand))]
public sealed class ReviewPullRequestCommandTest
{
    private readonly IGitGetChangesHandler _getChangesHandler = Substitute.For<IGitGetChangesHandler>();
    private readonly ILoginUseCase _loginUseCase = Substitute.For<ILoginUseCase>();
    private readonly IReviewFeedbackBuilder _reviewFeedbackBuilder = Substitute.For<IReviewFeedbackBuilder>();

    private ReviewPullRequestCommand CreateCommand() =>
        new(
            getChangesHandler: _getChangesHandler,
            loginUseCase: _loginUseCase,
            reviewFeedbackBuilder: _reviewFeedbackBuilder);

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
        _reviewFeedbackBuilder.DidNotReceiveWithAnyArgs().Run([]);
    }
}
