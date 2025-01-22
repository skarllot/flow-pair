using System.Collections.Immutable;
using Raiqub.LlmTools.FlowPair.Agent.Infrastructure;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Services;
using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;

public interface ICreateUnitTestChatScript : IProcessableChatScript<CreateUnitTestRequest, CreateUnitTestResponse>;

public sealed class CreateUnitTestChatScript(
    AgentJsonContext jsonContext,
    IProjectFilesMessageFactory projectFilesMessageFactory,
    IDirectoryStructureMessageFactory directoryStructureMessageFactory)
    : ICreateUnitTestChatScript
{
    private const string CodeResponseKey = "Markdown";
    private const string JsonResponseKey = "JSON";

    public string Name => "Create unit tests chat script";
    public ImmutableArray<string> Extensions => KnownFileExtension.UnitTestable;

    public string SystemInstruction =>
        """
        You are an expert developer, your task is to create unit tests following the best practices.
        You are given a set of project files, containing the filenames and their contents.
        """;

    public ImmutableList<Instruction> Instructions =>
    [
        Instruction.StepInstruction.Of(
            "Create unit tests for the specified code"),
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
            "Return the entire final version of the unit tests file content inside a code block (```)"),
        Instruction.StepInstruction.Of(
            "Where the new file for the unit tests should be located " +
            "according to language and project standards?"),
        Instruction.JsonConvertInstruction.Of(
            JsonResponseKey,
            "Reply the file path in a valid JSON format. The schema of the JSON object must be:",
            FilePathResponse.Schema),
    ];

    public ImmutableList<Message> GetInitialMessages(CreateUnitTestRequest input) =>
    [
        projectFilesMessageFactory.CreateWithProjectFilesContent(input.RootPath),
        directoryStructureMessageFactory.CreateWithRepositoryStructure(input.RootPath),
        ..input.ExampleFileInfo is not null
            ?
            [
                new Message(
                    SenderRole.User,
                    $"""
                     Unit tests example:
                     ```
                     {input.ExampleFileInfo.ReadAllText()}
                     ```
                     """),
            ]
            : (ReadOnlySpan<Message>) [],
        new Message(
            SenderRole.User,
            $"""
             The source file to be tested is located at '{input.RootPath.GetRelativePath(input.FileInfo.FullName)}' and its content is:
             ```
             {input.FileInfo.ReadAllText()}
             ```
             """),
    ];

    public Result<object, string> Parse(string key, string input) => key switch
    {
        CodeResponseKey => MarkdownCodeExtractor
            .TryExtractSingle(input)
            .Select(static object (x) => x),
        JsonResponseKey => JsonContentDeserializer
            .TryDeserialize(input, jsonContext.FilePathResponse)
            .Select(static object (x) => x),
        _ => $"Unknown output key '{key}'",
    };

    public Option<CreateUnitTestResponse> CompileOutputs(ChatWorkspace chatWorkspace) =>
        from filePath in OutputProcessor.GetFirst<FilePathResponse>(chatWorkspace, JsonResponseKey)
        from codeSnippet in OutputProcessor.GetFirst<CodeSnippet>(chatWorkspace, CodeResponseKey)
        select new CreateUnitTestResponse(filePath.FilePath, codeSnippet.Content);
}
