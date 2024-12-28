using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Agent.Models;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;

public static class UnitTestChatScript
{
    public static readonly ImmutableList<ChatScript> Default =
    [
        new(
            "Generic programming chat script",
            [
                /* Python          */".py", ".pyw", ".pyx", ".pxd", ".pxi",
                /* JavaScript      */".js", ".jsx", ".mjs", ".cjs",
                /* Java            */".java",
                /* C#              */".cs", ".csx",
                /* C++             */".cpp", ".cxx", ".cc", ".c++", ".hpp", ".hxx", ".h", ".hh", ".h++",
                /* PHP             */".php", ".phtml", ".phps",
                /* Ruby            */".rb", ".rbw", ".rake",
                /* Swift           */".swift",
                /* R               */".r",
                /* SQL             */".sql",
                /* Kotlin          */".kt", ".kts",
                /* TypeScript      */".ts", ".tsx",
                /* Go (Golang)     */".go",
                /* Rust            */".rs",
                /* Scala           */".scala", ".sc",
                /* Dart            */".dart",
                /* Perl            */".pl", ".pm", ".t", ".pod",
                /* MATLAB          */".m",
            ],
            """
            You are an expert developer, your task is to create unit tests following the best practices.
            You are given a set of project files, containing the filenames and their contents.
            """,
            [
                Instruction.StepInstruction.Of(
                    "Ensure the unit test cover every path"),
                Instruction.StepInstruction.Of(
                    "Ensure the unit test does not create any mutants on mutation analysis"),
                Instruction.JsonConvertInstruction.Of(
                    """
                    Copy the unit test content in a valid JSON format.
                    The "content" property can be multiline with entire unit test content.
                    The schema of the JSON object must be:
                    """),
            ]),
    ];
}
