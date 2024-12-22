using System.Collections.Immutable;
using System.IO.Abstractions;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using LibGit2Sharp;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Git.GetChanges;

public partial interface IGitGetChangesHandler;

[GenerateAutomaticInterface]
public class GitGetChangesHandler(
    IFileSystem fileSystem,
    IAnsiConsole console)
    : IGitGetChangesHandler
{
    public Option<ImmutableList<FileChange>> Extract(string? path)
    {
        path = TryFindRepository(fileSystem, path).UnwrapOrNull();
        if (path is null)
        {
            console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
            return None;
        }

        using var repo = new Repository(path);
        var builder = ImmutableList.CreateBuilder<FileChange>();

        FillChanges(repo, builder, DiffTargets.Index);

        if (builder.Count == 0)
        {
            FillChanges(repo, builder, DiffTargets.WorkingDirectory);
        }

        if (builder.Count == 0)
        {
            FillChangesFromLastCommit(repo, builder);
        }

        console.MarkupLine($"Found {builder.Count} changed files");
        return Some(builder.ToImmutable());
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
        if (parentCommit is null)
        {
            return;
        }

        foreach (var changes in repo.Diff
                     .Compare<Patch>(parentCommit.Tree, lastCommit.Tree)
                     .Where(p => !p.IsBinaryComparison && p.Status != ChangeKind.Deleted && p.Mode != Mode.Directory))
        {
            builder.Add(new FileChange(changes.Path, changes.Patch));
        }
    }

    private static Option<string> TryFindRepository(IFileSystem fileSystem, string? path)
    {
        var currentDirectory = fileSystem.DirectoryInfo.New(path ?? fileSystem.Directory.GetCurrentDirectory());
        while (currentDirectory != null)
        {
            if (currentDirectory
                .EnumerateDirectories(".git", SearchOption.TopDirectoryOnly)
                .Any())
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return None;
    }
}
