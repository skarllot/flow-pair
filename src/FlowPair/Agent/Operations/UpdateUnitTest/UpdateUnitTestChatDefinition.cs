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
                "Ensure the unit tests cover every possible execution path in the code"),
            Instruction.StepInstruction.Of(
                "Ensure the unit tests are sensitive to mutations in the source code. " +
                "When mutation testing introduces small changes to the implementation (mutants), " +
                "at least one test should fail. " +
                "This verifies that the tests can detect potential bugs or behavioral changes."),
            Instruction.StepInstruction.Of(
                "Remove any redundant tests while maintaining full coverage"),
            Instruction.CodeExtractInstruction.Of(
                CodeResponseKey,
                "Return the entire final version of the updated unit tests file content, " +
                "incorporating all the above improvements, inside a code block (```)"),
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
