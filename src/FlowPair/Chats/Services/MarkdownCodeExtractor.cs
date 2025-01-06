using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Common;
using FxKit.Parsers;

namespace Ciandt.FlowTools.FlowPair.Chats.Services;

public static partial class MarkdownCodeExtractor
{
    public static Result<ImmutableList<CodeSnippet>, string> TryExtract(string content)
    {
        return
            from value in StringParser.NonNullOrWhiteSpace(content)
                .OkOr("Code not found on empty content")
            select CodeBlockRegex().Matches(content)
                .Select(m => new CodeSnippet(m.Groups[2].Value, m.Groups[1].Value))
                .ToImmutableList();
    }

    public static Result<CodeSnippet, string> TryExtractSingle(string content)
    {
        return
            from value in StringParser.NonNullOrWhiteSpace(content)
                .OkOr("Code not found on empty content")
            from code in CodeBlockRegex().Matches(content)
                .Select(m => new CodeSnippet(m.Groups[2].Value, m.Groups[1].Value))
                .TrySingle()
                .MapErr(
                    p => p.Match(
                        Empty: "No code block found",
                        MoreThanOneElement: "More than one code block found"))
            select code;
    }

    [GeneratedRegex(@"```(\w*)\s*\n([\s\S]*?)\r?\n\s*```", RegexOptions.Singleline)]
    private static partial Regex CodeBlockRegex();
}
