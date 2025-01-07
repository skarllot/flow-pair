using System.Collections.Immutable;
using AutomaticInterface;
using LibGit2Sharp;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Git.GetChanges;

public partial interface IGitGetChangesHandler;

[GenerateAutomaticInterface]
public class GitGetChangesHandler(
    IAnsiConsole console,
    IWorkingDirectoryWalker workingDirectoryWalker)
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

        using var repo = new Repository(gitRootDir.FullName);
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

    private bool FillChangesFromCommit(Repository repo, ImmutableList<FileChange>.Builder builder, string commit)
    {
        var foundCommit = repo.Commits
            .FirstOrDefault(c => c.Sha.StartsWith(commit, StringComparison.OrdinalIgnoreCase));
        if (foundCommit is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate specified commit.");
            return false;
        }

        var parentCommit = foundCommit.Parents.FirstOrDefault();

        foreach (var changes in repo.Diff
                     .Compare<Patch>(parentCommit?.Tree, foundCommit.Tree)
                     .Where(p => !p.IsBinaryComparison && p.Status != ChangeKind.Deleted && p.Mode != Mode.Directory))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }

        return true;
    }

    private static void FillChangesFallback(Repository repo, ImmutableList<FileChange>.Builder builder)
    {
        FillChanges(repo, builder, DiffTargets.Index);

        if (builder.Count == 0)
        {
            FillChanges(repo, builder, DiffTargets.WorkingDirectory);
        }

        if (builder.Count == 0)
        {
            FillChangesFromLastCommit(repo, builder);
        }
    }

    private static void FillChanges(Repository repo, ImmutableList<FileChange>.Builder builder, DiffTargets diffTargets)
    {
        foreach (var changes in repo.Diff
                     .Compare<Patch>(repo.Head.Tip?.Tree, diffTargets)
                     .Where(p => !p.IsBinaryComparison && p.Status != ChangeKind.Deleted && p.Mode != Mode.Directory))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }
    }

    private static void FillChangesFromLastCommit(Repository repo, ImmutableList<FileChange>.Builder builder)
    {
        var lastCommit = repo.Head.Tip;
        var parentCommit = lastCommit.Parents.FirstOrDefault();

        foreach (var changes in repo.Diff
                     .Compare<Patch>(parentCommit?.Tree, lastCommit.Tree)
                     .Where(p => !p.IsBinaryComparison && p.Status != ChangeKind.Deleted && p.Mode != Mode.Directory))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }
    }
}
