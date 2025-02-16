using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.GetChanges;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewPullRequest;

public sealed class ReviewPullRequestCommand(
    IGitGetChangesHandler getChangesHandler,
    ILoginUseCase loginUseCase,
    IReviewFeedbackBuilder reviewFeedbackBuilder)
{
    /// <summary>
    /// Review a pull request.
    /// </summary>
    /// <param name="sourceBranch">-sb, The name of the source branch (e.g. main).</param>
    /// <param name="targetBranch">-tb, The name of the target branch (e.g. feature-1).</param>
    /// <param name="path">Path to the repository.</param>
    /// <returns></returns>
    [Command("review pr")]
    public int Execute(
        string? sourceBranch = null,
        string? targetBranch = null,
        [Argument] string? path = null)
    {
        return (from diff in getChangesHandler.ExtractFromBranchesDiff(path, sourceBranch, targetBranch).OkOr(0)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 1)
                from feedback in reviewFeedbackBuilder.Run(diff).OkOr(2)
                select 0)
            .UnwrapEither();
    }
}
