using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;

public sealed class ReviewChangesCommand(
    IGitGetChangesHandler getChangesHandler,
    ILoginUseCase loginUseCase,
    IReviewFeedbackBuilder reviewFeedbackBuilder)
{
    /// <summary>
    /// Review changed files.
    /// </summary>
    /// <param name="path">Path to the repository.</param>
    /// <param name="commit">-c, Commit hash.</param>
    [Command("review")]
    public int Execute(
        [Argument] string? path = null,
        string? commit = null)
    {
        return (from diff in getChangesHandler.Extract(path, commit).OkOr(0)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 1)
                from feedback in reviewFeedbackBuilder.Run(diff).OkOr(2)
                select 0)
            .UnwrapEither();
    }
}
