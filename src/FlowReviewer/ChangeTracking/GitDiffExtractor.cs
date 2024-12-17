using System.Collections.Immutable;
using System.IO.Abstractions;
using AutomaticInterface;
using LibGit2Sharp;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.ChangeTracking;

public partial interface IGitDiffExtractor;

[GenerateAutomaticInterface]
public class GitDiffExtractor(IFileSystem fileSystem, IAnsiConsole console)
    : IGitDiffExtractor
{
    public Option<ImmutableList<FileChange>> Extract()
    {
        using var repo = new Repository(fileSystem.Directory.GetCurrentDirectory());
        var builder = ImmutableList.CreateBuilder<FileChange>();

        FillChanges(repo, builder, DiffTargets.Index);

        if (builder.Count == 0)
        {
            FillChanges(repo, builder, DiffTargets.WorkingDirectory);
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
}
