using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;

public interface IUpdateUnitTestChatDefinition : IChatDefinition<UpdateUnitTestResponse>;

public sealed class UpdateUnitTestChatDefinition
    : IUpdateUnitTestChatDefinition
{
    private const string CodeResponseKey = "Markdown";

    public ChatScript ChatScript { get; } = new(
        "Update unit tests chat script",
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
                "Update the unit tests for the specified source code"),
            Instruction.StepInstruction.Of(
                "Ensure the unit test cover every path"),
            Instruction.StepInstruction.Of(
                "Ensure the unit test does not create any mutants on mutation analysis"),
            Instruction.StepInstruction.Of(
                "If any test is redundant remove it"),
            Instruction.CodeExtractInstruction.Of(
                CodeResponseKey,
                "Return the entire updated unit tests file content inside of a code block (```)"),
        ]);

    public Result<object, string> Parse(string key, string input) => key switch
    {
        CodeResponseKey => MarkdownCodeExtractor
            .TryExtractSingle(input)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'"
    };

    public Option<UpdateUnitTestResponse> ConvertResult(ChatWorkspace chatWorkspace) =>
        from code in OutputProcessor.GetFirst<CodeSnippet>(chatWorkspace, CodeResponseKey)
        select new UpdateUnitTestResponse(code.Content);
}
