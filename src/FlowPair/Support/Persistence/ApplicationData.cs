using System.IO.Abstractions;

namespace Raiqub.LlmTools.FlowPair.Support.Persistence;

public static class ApplicationData
{
    private const string AppName = "FlowPair";

    public static IDirectoryInfo GetAppDataDirectory(IFileSystem fileSystem) => fileSystem.DirectoryInfo.New(
        fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName));

    public static IDirectoryInfo GetTempPath(IFileSystem fileSystem) => fileSystem.DirectoryInfo.New(
        fileSystem.Path.Combine(
            fileSystem.Path.GetTempPath(),
            AppName));
}
