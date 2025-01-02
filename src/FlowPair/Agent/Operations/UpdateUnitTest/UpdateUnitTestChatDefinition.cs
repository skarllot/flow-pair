using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Models;
using Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Services;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest;

public interface IUpdateUnitTestChatDefinition : IChatDefinition<UpdateUnitTestResponse>;

public sealed class UpdateUnitTestChatDefinition(
    AgentJsonContext jsonContext)
    : IUpdateUnitTestChatDefinition
{
    public const string JsonResponseKey = "UpdateUnitTestResponse";

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
            Instruction.JsonConvertInstruction.Of(
                JsonResponseKey,
                """
                Copy the generated unit tests content in a valid JSON format.
                The "content" property can be multiline with entire unit test content.
                The schema of the JSON object must be:
                """,
                UpdateUnitTestResponse.Schema),
        ]);

    public Result<object, string> Parse(string key, string input) => key switch
    {
        JsonResponseKey => ContentDeserializer
            .TryDeserialize(input, jsonContext.UpdateUnitTestResponse)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'"
    };

    public Option<UpdateUnitTestResponse> ConvertResult(ChatWorkspace chatWorkspace) =>
        OutputProcessor.GetFirst<UpdateUnitTestResponse>(chatWorkspace, JsonResponseKey);
}
