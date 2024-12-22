using System.IO.Abstractions;
using System.Text;

namespace Ciandt.FlowTools.FlowPair.Common;

public static class FileSystemExtensions
{
    public static IFileInfo NewFile(this IDirectoryInfo directoryInfo, string fileName)
    {
        var fileSystem = directoryInfo.FileSystem;
        return fileSystem.FileInfo.New(fileSystem.Path.Combine(directoryInfo.FullName, fileName));
    }

    public static Option<IFileInfo> NewFileIfExists(this IDirectoryInfo directoryInfo, string fileName)
    {
        var fileSystem = directoryInfo.FileSystem;
        var fileInfo = fileSystem.FileInfo.New(fileSystem.Path.Combine(directoryInfo.FullName, fileName));
        return fileInfo.Exists ? Some(fileInfo) : None;
    }

    public static void WriteAllText(this IFileInfo fileInfo, string? contents, Encoding encoding)
    {
        fileInfo.FileSystem.File.WriteAllText(fileInfo.FullName, contents, encoding);
    }
}
