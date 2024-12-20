using System.IO.Abstractions;

namespace Ciandt.FlowTools.FlowPair.Persistence;

public static class ApplicationData
{
    private const string AppName = "FlowPair";

    public static string GetPath(IFileSystem fileSystem) => fileSystem.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);

    public static string GetTempPath(IFileSystem fileSystem) => fileSystem.Path.Combine(
        fileSystem.Path.GetTempPath(),
        AppName);
}
