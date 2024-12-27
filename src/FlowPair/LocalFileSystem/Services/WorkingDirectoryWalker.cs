using System.IO.Abstractions;
using AutomaticInterface;

namespace Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;

public partial interface IWorkingDirectoryWalker;

[GenerateAutomaticInterface]
public sealed class WorkingDirectoryWalker(
    IFileSystem fileSystem)
    : IWorkingDirectoryWalker
{
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
}
