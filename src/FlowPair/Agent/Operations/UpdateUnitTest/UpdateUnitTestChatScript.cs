using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;

public interface IUpdateUnitTestChatScript : IProcessableChatScript<UpdateUnitTestRequest, UpdateUnitTestResponse>;

public sealed class UpdateUnitTestChatScript(
    IProjectFilesMessageFactory projectFilesMessageFactory,
    IDirectoryStructureMessageFactory directoryStructureMessageFactory)
    : IUpdateUnitTestChatScript
{
    private const string CodeResponseKey = "Markdown";

    public string Name => "Update unit tests chat script";
    public ImmutableArray<string> Extensions => KnownFileExtension.UnitTestable;

    public string SystemInstruction =>
        """
        You are an expert developer, your task is to create unit tests following the best practices.
        You are given a set of project files, containing the filenames and their contents.
        """;

    public ImmutableList<Instruction> Instructions =>
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
    ];

    public ImmutableList<Message> GetInitialMessages(UpdateUnitTestRequest input) =>
    [
        projectFilesMessageFactory.CreateWithProjectFilesContent(input.RootPath),
        directoryStructureMessageFactory.CreateWithRepositoryStructure(input.RootPath),
        new Message(
            SenderRole.User,
            $"""
             The source file updated content is:
             ```
             {input.SourceFileInfo.ReadAllText()}
             ```
             """),
        new Message(
            SenderRole.User,
            $"""
             The existing test file content is:
             ```
             {input.TestFileInfo.ReadAllText()}
             ```
             """),
    ];

    public Result<object, string> Parse(string key, string input) => key switch
    {
        CodeResponseKey => MarkdownCodeExtractor
            .TryExtractSingle(input)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'",
    };

    public Option<UpdateUnitTestResponse> CompileOutputs(ChatWorkspace chatWorkspace) =>
        from code in OutputProcessor.GetFirst<CodeSnippet>(chatWorkspace, CodeResponseKey)
        select new UpdateUnitTestResponse(code.Content);
}
