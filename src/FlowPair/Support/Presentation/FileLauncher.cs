using System.Diagnostics;

namespace Ciandt.FlowTools.FlowPair.Support.Presentation;

public static class FileLauncher
{
    public static void OpenFile(string filePath)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo { FileName = filePath, UseShellExecute = true };
        process.Start();
    }
}
