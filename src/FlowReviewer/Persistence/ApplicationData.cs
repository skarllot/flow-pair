using System.IO.Abstractions;

namespace Ciandt.FlowTools.FlowReviewer.Persistence;

public static class ApplicationData
{
    private const string AppName = "FlowReviewer";

    public static string GetPath(IFileSystem fileSystem) => fileSystem.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);
}
