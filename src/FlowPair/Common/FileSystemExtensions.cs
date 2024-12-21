using System.IO.Abstractions;
using System.Text;

namespace Ciandt.FlowTools.FlowPair.Common;

public static class FileSystemExtensions
{
    public static IFileInfo CreateFile(this IDirectoryInfo directoryInfo, string fileName)
    {
        var fileSystem = directoryInfo.FileSystem;
        return fileSystem.FileInfo.New(fileSystem.Path.Combine(directoryInfo.FullName, fileName));
    }

    public static void WriteAllText(this IFileInfo fileInfo, string? contents, Encoding encoding)
    {
        fileInfo.FileSystem.File.WriteAllText(fileInfo.FullName, contents, encoding);
    }
}
