using System.Diagnostics;
using System.IO.Abstractions;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Support.Presentation;

public static class FileLauncher
{
    public static Unit LaunchFile(this IFileInfo fileInfo, IAnsiConsole console)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo { FileName = fileInfo.FullName, UseShellExecute = true };
            process.Start();
        }
        catch (Exception e)
        {
            console.WriteException(e);
        }

        return Unit();
    }
}
