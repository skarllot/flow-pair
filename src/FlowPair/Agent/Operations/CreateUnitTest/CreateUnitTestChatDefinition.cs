using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;

public interface ICreateUnitTestChatDefinition : IChatDefinition<CreateUnitTestResponse>;

public sealed class CreateUnitTestChatDefinition(
    AgentJsonContext jsonContext)
    : ICreateUnitTestChatDefinition
{
    private const string CodeResponseKey = "Markdown";
    private const string JsonResponseKey = "JSON";

    public ChatScript ChatScript { get; } = new(
        "Create unit tests chat script",
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
                "Create unit tests for the specified code"),
            Instruction.StepInstruction.Of(
                "Ensure the unit test cover every path"),
            Instruction.StepInstruction.Of(
                "Ensure the unit test does not create any mutants on mutation analysis"),
            Instruction.CodeExtractInstruction.Of(
                CodeResponseKey,
                "Return only the unit tests code inside a code block (```)"),
            Instruction.StepInstruction.Of(
                "Where the new file for the unit tests should be located " +
                "according to language and project standards?"),
            Instruction.JsonConvertInstruction.Of(
                JsonResponseKey,
                """
                Reply the file path in a valid JSON format.
                The schema of the JSON object must be:
                """,
                FilePathResponse.Schema),
        ]);

    public Result<object, string> Parse(string key, string input) => key switch
    {
        CodeResponseKey => MarkdownCodeExtractor
            .TryExtractSingle(input)
            .Select(static object (x) => x),
        JsonResponseKey => JsonContentDeserializer
            .TryDeserialize(input, jsonContext.FilePathResponse)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'"
    };

    public Option<CreateUnitTestResponse> ConvertResult(ChatWorkspace chatWorkspace) =>
        from filePath in OutputProcessor.GetFirst<FilePathResponse>(chatWorkspace, JsonResponseKey)
        from codeSnippet in OutputProcessor.GetFirst<CodeSnippet>(chatWorkspace, CodeResponseKey)
        select new CreateUnitTestResponse(filePath.FilePath, codeSnippet.Content);
}
