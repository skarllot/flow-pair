using System.Buffers;
using System.IO.Abstractions;
using System.Text;

namespace Raiqub.LlmTools.FlowPair.Common;

public static class FileSystemExtensions
{
    private const int DefaultBufferSize = 1024;

    public static string GetRelativePath(this IDirectoryInfo directory, string path)
    {
        return directory.FileSystem.Path.GetRelativePath(directory.FullName, path);
    }

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

    public static string ReadAllText(this IFileInfo fileInfo)
    {
        return fileInfo.FileSystem.File.ReadAllText(fileInfo.FullName);
    }

    public static void ReadAllTextTo(this IFileInfo fileInfo, StringBuilder sb)
    {
        using var stream = fileInfo.OpenRead();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var buffer = ArrayPool<char>.Shared.Rent(DefaultBufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = reader.Read(buffer.AsSpan(0, DefaultBufferSize))) != 0)
            {
                sb.Append(buffer.AsSpan(0, bytesRead));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
