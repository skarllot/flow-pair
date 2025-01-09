using Spectre.Console.Rendering;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests.Testing;

public static class RenderableExtensions
{
    public static string GetText(this IRenderable renderable)
    {
        using var testConsole = new TestConsole();
        testConsole.Write(renderable);

        return testConsole.Output;
    }
}
