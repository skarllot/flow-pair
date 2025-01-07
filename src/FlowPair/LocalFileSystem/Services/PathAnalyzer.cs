using System.Text.RegularExpressions;

namespace Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;

public static partial class PathAnalyzer
{
    public static string Normalize(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return path.Replace('/', '\\');
        }
        else
        {
            var tmp = path.AsSpan();
            if (DriveLetterRegex().IsMatch(tmp))
            {
                tmp = tmp[2..];

                if (tmp.IsEmpty)
                    return "/";
            }

            return string.Create(
                length: tmp.Length,
                state: tmp,
                action: static (dest, src) => src.Replace(dest, '\\', '/'));
        }
    }

    [GeneratedRegex("^[a-zA-Z]:")]
    private static partial Regex DriveLetterRegex();
}
