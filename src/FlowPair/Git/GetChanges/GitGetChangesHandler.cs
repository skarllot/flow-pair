using System.Collections.Immutable;
using AutomaticInterface;
using LibGit2Sharp;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Git.Services;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Git.GetChanges;

public partial interface IGitGetChangesHandler;

[GenerateAutomaticInterface]
public class GitGetChangesHandler(
    IAnsiConsole console,
    IWorkingDirectoryWalker workingDirectoryWalker,
    IGitRepositoryFactory repositoryFactory)
    : IGitGetChangesHandler
{
    public Option<ImmutableList<FileChange>> Extract(string? path, string? commit)
    {
        var gitRootDir = workingDirectoryWalker.TryFindRepositoryRoot(path).UnwrapOrNull();
        if (gitRootDir is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
            return None;
        }

        using var repo = repositoryFactory.Create(gitRootDir);
        var builder = ImmutableList.CreateBuilder<FileChange>();

        if (!string.IsNullOrEmpty(commit))
        {
            if (!FillChangesFromCommit(repo, builder, commit))
            {
                return None;
            }
        }
        else
        {
            FillChangesFallback(repo, builder);
        }

        console.MarkupLine($"Found {builder.Count} changed files");
        return Some(builder.ToImmutable());
    }

    public Option<ImmutableList<FileChange>> ExtractFromBranchesDiff(
        string? path,
        string? sourceBranch,
        string? targetBranch)
    {
        var gitRootDir = workingDirectoryWalker.TryFindRepositoryRoot(path).UnwrapOrNull();
        if (gitRootDir is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
            return None;
        }

        using var repo = repositoryFactory.Create(gitRootDir);
        var builder = ImmutableList.CreateBuilder<FileChange>();

        var repoSourceBranch = sourceBranch is not null
            ? repo.Branches
                .FirstOrDefault(b => b.FriendlyName.Equals(sourceBranch, StringComparison.OrdinalIgnoreCase))
            : repo.Branches
                .FirstOrDefault(b => b.FriendlyName is "main" or "master");
        if (repoSourceBranch is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate source branch.");
            return None;
        }

        var repoTargetBranch = targetBranch is not null
            ? repo.Branches
                .FirstOrDefault(b => b.FriendlyName.Equals(targetBranch, StringComparison.OrdinalIgnoreCase))
            : repo.Head;
        if (repoTargetBranch is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate target branch.");
            return None;
        }

        FillChangesBetweenCommits(builder, repo, repoSourceBranch.Tip, repoTargetBranch.Tip);

        console.MarkupLine($"Found {builder.Count} changed files");
        return Some(builder.ToImmutable());
    }

    private bool FillChangesFromCommit(IRepository repo, ImmutableList<FileChange>.Builder builder, string commit)
    {
        var foundCommit = repo.Commits
            .FirstOrDefault(c => c.Sha.StartsWith(commit, StringComparison.OrdinalIgnoreCase));
        if (foundCommit is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate specified commit.");
            return false;
        }

        if (!FillChangesBetweenCommits(builder, repo, null, foundCommit))
        {
            console.MarkupLine("[red]Error:[/] The specified commit has multiple parents.");
            return false;
        }

        return true;
    }

    private static void FillChangesFallback(IRepository repo, ImmutableList<FileChange>.Builder builder)
    {
        FillChanges(repo, builder, DiffTargets.Index);

        if (builder.Count == 0)
        {
            FillChanges(repo, builder, DiffTargets.WorkingDirectory);
        }

        if (builder.Count == 0)
        {
            FillChangesBetweenCommits(builder, repo, null, repo.Head.Tip);
        }
    }

    private static void FillChanges(
        IRepository repo,
        ImmutableList<FileChange>.Builder builder,
        DiffTargets diffTargets)
    {
        foreach (var changes in repo.Diff
                     .Compare<Patch>(repo.Head.Tip?.Tree, diffTargets)
                     .Where(PatchSpecs.IsChangedCode))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }
    }

    private static bool FillChangesBetweenCommits(
        ImmutableList<FileChange>.Builder builder,
        IRepository repo,
        Commit? sourceCommit,
        Commit targetCommit)
    {
        Patch? patch;
        if (sourceCommit is null)
        {
            patch = targetCommit.Parents.TrySingle()
                .Match(
                    p => repo.Diff.Compare<Patch>(p.Tree, targetCommit.Tree),
                    e => e == SingleElementProblem.Empty
                        ? repo.Diff.Compare<Patch>(null, targetCommit.Tree)
                        : null);
        }
        else
        {
            patch = repo.Diff.Compare<Patch>(sourceCommit.Tree, targetCommit.Tree);
        }

        if (patch is null)
        {
            return false;
        }

        foreach (var changes in patch.Where(PatchSpecs.IsChangedCode))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }

        return true;
    }
}
