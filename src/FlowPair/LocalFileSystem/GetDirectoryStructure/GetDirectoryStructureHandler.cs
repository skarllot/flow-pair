using System.Collections.Frozen;
using System.Globalization;
using System.IO.Abstractions;
using System.Text;
using AutomaticInterface;

namespace Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;

public partial interface IGetDirectoryStructureHandler;

[GenerateAutomaticInterface]
public sealed class GetDirectoryStructureHandler
    : IGetDirectoryStructureHandler
{
    private static readonly FrozenSet<string> s_ignoreList = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        [
            "__pycache__", "artifacts", "bin", "blib", "build", "cache", "dist", "node_modules", "obj", "out", "pkg",
            "Pods", "project", "publish", "target", "vendor", "venv", "xcuserdata"
        ]);

    public string Execute(IDirectoryInfo directoryInfo)
    {
        var result = new StringBuilder();
        FillDirectories(result, directoryInfo, 0);
        return result.ToString();
    }

    private static void FillDirectories(StringBuilder builder, IDirectoryInfo directoryInfo, int level)
    {
        PrintLevel(builder, level);
        builder.Append(CultureInfo.InvariantCulture, $"{directoryInfo.Name}/");

        foreach (var item in directoryInfo.EnumerateDirectories().Where(IsUserDirectory))
        {
            FillDirectories(builder, item, level + 1);
        }
    }

    private static bool IsUserDirectory(IDirectoryInfo x)
    {
        return !x.Name.StartsWith('.') && !s_ignoreList.Contains(x.Name);
    }

    private static void PrintLevel(StringBuilder builder, int level)
    {
        if (level == 0)
            return;

        builder.AppendLine();
        builder.Append(string.Join(null, Enumerable.Repeat("|   ", level - 1)));
        builder.Append("|-- ");
    }
}
