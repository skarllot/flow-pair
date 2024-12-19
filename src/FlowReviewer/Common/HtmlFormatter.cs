using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Ciandt.FlowTools.FlowReviewer.Common;

public static partial class HtmlFormatter
{
    public static string EncodeToHtml(string text)
    {
        text = HttpUtility.HtmlEncode(text);
        text = CodeDelimiterRegex().Replace(text, """<pre><code class="language-$1">$2</code></pre>""");
        text = CodeBlockDelimiterRegex().Replace(text, "<code>$1</code>");
        text = ReplaceLineBreaksExceptInCode(text);
        return text;
    }

    private static string ReplaceLineBreaksExceptInCode(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        var insideCodeBlock = false;
        var lastSize = 0;

        foreach (var line in input.AsSpan().EnumerateLines())
        {
            if (line.Contains("<code", StringComparison.OrdinalIgnoreCase))
                insideCodeBlock = true;

            if (!insideCodeBlock)
            {
                result.Append(line.TrimEnd());
                result.Append("<br>");
                lastSize = 4;
            }
            else
            {
                result.Append(line);
                result.Append('\n');
                lastSize = 1;
            }

            if (line.Contains("</code>", StringComparison.OrdinalIgnoreCase))
                insideCodeBlock = false;
        }

        result.Length -= lastSize;
        return result.ToString();
    }

    [GeneratedRegex(@"```(\S+)?(.*?)```", RegexOptions.Multiline)]
    private static partial Regex CodeDelimiterRegex();

    [GeneratedRegex("`(.*?)`", RegexOptions.Singleline)]
    private static partial Regex CodeBlockDelimiterRegex();
}
