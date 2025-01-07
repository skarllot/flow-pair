using System.IO.Abstractions;
using AutomaticInterface;

namespace Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

public partial interface IWorkingDirectoryWalker;

[GenerateAutomaticInterface]
public sealed class WorkingDirectoryWalker(
    IFileSystem fileSystem)
    : IWorkingDirectoryWalker
{
    private static readonly EnumerationOptions s_fileEnumerationOptions = new()
    {
        IgnoreInaccessible = true,
        MatchCasing = MatchCasing.CaseInsensitive,
        MatchType = MatchType.Simple,
        RecurseSubdirectories = true
    };

    public Option<IDirectoryInfo> TryFindRepositoryRoot(string? path)
    {
        var currentDirectory = fileSystem.DirectoryInfo.New(path ?? fileSystem.Directory.GetCurrentDirectory());
        if (!currentDirectory.Exists)
        {
            return None;
        }

        while (currentDirectory != null)
        {
            if (currentDirectory
                .EnumerateDirectories(".git", SearchOption.TopDirectoryOnly)
                .Any())
            {
                return Some(currentDirectory);
            }

            currentDirectory = currentDirectory.Parent;
        }

        return None;
    }

    public IEnumerable<IFileInfo> FindFilesByExtension(IDirectoryInfo rootDirectory, IEnumerable<string> extensions)
    {
        return extensions
            .SelectMany(e => rootDirectory.EnumerateFiles($"*{e}", s_fileEnumerationOptions));
    }

    public IEnumerable<IFileInfo> FindFilesByName(IDirectoryInfo rootDirectory, IEnumerable<string> filenames)
    {
        return filenames
            .SelectMany(e => rootDirectory.EnumerateFiles(e, s_fileEnumerationOptions));
    }
}
