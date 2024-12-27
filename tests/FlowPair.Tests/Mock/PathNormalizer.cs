using System.Text.RegularExpressions;

namespace Ciandt.FlowTools.FlowPair.Tests.Mock;

public static partial class PathNormalizer
{
    public static string FromWindows(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return path;
        }

        var result = path;
        if (AbsolutePathRegex().IsMatch(path))
        {
            result = AbsolutePathRegex().Replace(path, "/$2");
        }

        return result.Replace('\\', '/');
    }

    [GeneratedRegex("^([a-zA-Z]:)(.*)")]
    private static partial Regex AbsolutePathRegex();
}
