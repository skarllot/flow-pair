using System.IO.Abstractions;
using System.Text;
using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;

public sealed class UpdateUnitTestCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    IUpdateUnitTestChatScript chatScript,
    IWorkingDirectoryWalker workingDirectoryWalker,
    ILoginUseCase loginUseCase,
    IChatService chatService)
{
    /// <summary>
    /// Update existing unit test with code changes.
    /// </summary>
    /// <param name="sourceFile">-s, The file path of the code to test.</param>
    /// <param name="testFile">-t, The file path of the existing unit tests.</param>
    [Command("unittest update")]
    public int Execute(
        string sourceFile,
        string testFile)
    {
        var sourceFileInfo = fileSystem.FileInfo.New(PathAnalyzer.Normalize(sourceFile));
        if (!sourceFileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified source file does not exist.");
            return 1;
        }

        var testFileInfo = fileSystem.FileInfo.New(PathAnalyzer.Normalize(testFile));
        if (!testFileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified test file does not exist.");
            return 2;
        }

        return (from rootPath in workingDirectoryWalker.TryFindRepositoryRoot(sourceFileInfo.Directory?.FullName)
                    .OkOrElse(HandleFindRepositoryRootError)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 4)
                let input = new UpdateUnitTestRequest(sourceFileInfo, testFileInfo, rootPath)
                from response in chatService
                    .Run(input, console.Progress(), LlmModelType.Claude35Sonnet, chatScript)
                    .MapErr(HandleChatServiceError)
                let create = RewriteUnitTestFile(rootPath, testFileInfo, response)
                select 0)
            .UnwrapEither();
    }

    private int HandleFindRepositoryRootError()
    {
        console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
        return 3;
    }

    private int HandleChatServiceError(string errorMessage)
    {
        console.MarkupLineInterpolated($"[red]Error:[/] {errorMessage}");
        return 5;
    }

    private Unit RewriteUnitTestFile(IDirectoryInfo rootPath, IFileInfo testFileInfo, UpdateUnitTestResponse response)
    {
        var fileRelativePath = rootPath.GetRelativePath(testFileInfo.FullName);
        testFileInfo.WriteAllText(response.Content, Encoding.UTF8);
        console.MarkupLineInterpolated($"[green]Unit tests updated:[/] {fileRelativePath}");
        return Unit();
    }
}
