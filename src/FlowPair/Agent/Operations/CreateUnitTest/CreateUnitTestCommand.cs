using System.IO.Abstractions;
using System.Text;
using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;

public sealed class CreateUnitTestCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    ICreateUnitTestChatScript chatScript,
    IWorkingDirectoryWalker workingDirectoryWalker,
    ILoginUseCase loginUseCase,
    IChatService chatService)
{
    /// <summary>
    /// Create unit test for the code on the file.
    /// </summary>
    /// <param name="filePath">-f, The file path of the code to test.</param>
    /// <param name="exampleFilePath">-e, The example unit test file path.</param>
    [Command("unittest create")]
    public int Execute(
        string filePath,
        string? exampleFilePath = null)
    {
        var fileInfo = fileSystem.FileInfo.New(PathAnalyzer.Normalize(filePath));
        if (!fileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified file does not exist.");
            return 1;
        }

        var exampleFileInfo = exampleFilePath is not null
            ? fileSystem.FileInfo.New(PathAnalyzer.Normalize(exampleFilePath))
            : null;
        if (exampleFileInfo is not null && !exampleFileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified example file does not exist.");
            return 2;
        }

        return (from rootPath in workingDirectoryWalker.TryFindRepositoryRoot(fileInfo.Directory?.FullName)
                    .OkOrElse(HandleFindRepositoryRootError)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 4)
                let input = new CreateUnitTestRequest(fileInfo, exampleFileInfo, rootPath)
                from response in chatService
                    .Run(input, console.Progress(), LlmModelType.Claude35Sonnet, chatScript)
                    .MapErr(HandleChatServiceError)
                let testFile = CreateUnitTestFile(rootPath, response)
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

    private Unit CreateUnitTestFile(IDirectoryInfo rootPath, CreateUnitTestResponse response)
    {
        var normalizedFilePath = PathAnalyzer.Normalize(response.FilePath);

        var testPath = fileSystem.Path.Combine(rootPath.FullName, normalizedFilePath);

        var dirPath = fileSystem.Path.GetDirectoryName(testPath);
        if (dirPath != null && !fileSystem.Directory.Exists(dirPath))
            fileSystem.Directory.CreateDirectory(dirPath);

        fileSystem.File.WriteAllText(testPath, response.Content, Encoding.UTF8);
        console.MarkupLineInterpolated($"[green]File created:[/] {response.FilePath}");
        return Unit();
    }
}
