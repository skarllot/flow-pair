using System.Globalization;
using System.Reflection;
using System.Text;

namespace Raiqub.LlmTools.FlowPair.Tests.Testing;

public sealed class ResourceReader(Assembly assembly)
{
    private string[]? _resourceNames;

    private string[] ResourceNames => _resourceNames ??= assembly.GetManifestResourceNames();

    public string GetString(string resourceName, params object?[]? args)
    {
        return args is null || args.Length == 0
            ? LoadEmbeddedResource(FindResourceName(resourceName) ?? resourceName)
            : string.Format(
                CultureInfo.InvariantCulture,
                LoadEmbeddedResource(FindResourceName(resourceName) ?? resourceName),
                args);
    }

    private string? FindResourceName(string partialName) =>
        Array.Find(ResourceNames, n => n.EndsWith(partialName, StringComparison.Ordinal));

    private string LoadEmbeddedResource(string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new ArgumentException(
                $"Could not find embedded resource {resourceName}. " +
                $"Available names: {string.Join(", ", ResourceNames)}.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
