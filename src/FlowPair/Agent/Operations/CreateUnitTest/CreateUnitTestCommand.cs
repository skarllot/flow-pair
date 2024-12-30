using System.IO.Abstractions;
using System.Text;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;

public sealed class CreateUnitTestCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    AgentJsonContext jsonContext,
    IWorkingDirectoryWalker workingDirectoryWalker,
    IProjectFilesMessageFactory projectFilesMessageFactory,
    IDirectoryStructureMessageFactory directoryStructureMessageFactory,
    ILoginUseCase loginUseCase,
    IChatService chatService)
{
    /// <summary>
    /// Create unit test for the code on the file.
    /// </summary>
    /// <param name="filePath">-f, The file path of the code to test.</param>
    [Command("create-unittest")]
    public int Execute(
        string filePath)
    {
        var fileInfo = fileSystem.FileInfo.New(PathAnalyzer.Normalize(filePath));
        if (!fileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified file does not exist.");
            return 1;
        }

        return (from rootPath in workingDirectoryWalker.TryFindRepositoryRoot(fileInfo.Directory?.FullName)
                    .OkOrElse(HandleFindRepositoryRootError)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 3)
                let initialMessages = BuildInitialMessages(fileInfo, rootPath)
                from response in chatService.RunSingle(
                        console.Progress(),
                        AllowedModel.Claude35Sonnet,
                        UnitTestChatScript.Default[0],
                        initialMessages,
                        jsonContext.CreateUnitTestResponse)
                    .MapErr(HandleChatServiceError)
                let testFile = CreateUnitTestFile(rootPath, response)
                select 0)
            .UnwrapEither();
    }

    private int HandleFindRepositoryRootError()
    {
        console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
        return 2;
    }

    private int HandleChatServiceError(string errorMessage)
    {
        console.MarkupLineInterpolated($"[red]Error:[/] {errorMessage}");
        return 4;
    }

    private IEnumerable<Message> BuildInitialMessages(IFileInfo fileInfo, IDirectoryInfo rootPath)
    {
        yield return projectFilesMessageFactory.CreateWithProjectFilesContent(rootPath);
        yield return directoryStructureMessageFactory.CreateWithRepositoryStructure(rootPath);

        yield return new Message(
            Role.User,
            $"""
             The source file to be tested is located at '{rootPath.GetRelativePath(fileInfo.FullName)}' and its content is:
             ```
             {fileInfo.ReadAllText()}
             ```
             """);
    }

    private Unit CreateUnitTestFile(IDirectoryInfo rootPath, CreateUnitTestResponse response)
    {
        var normalizedFilePath = PathAnalyzer.Normalize(response.FilePath);

        var testPath = fileSystem.Path.Combine(rootPath.FullName, normalizedFilePath);

        var dirPath = fileSystem.Path.GetDirectoryName(testPath);
        if (dirPath != null && !fileSystem.Directory.Exists(dirPath))
            fileSystem.Directory.CreateDirectory(dirPath);

        fileSystem.File.WriteAllText(testPath, response.Content, Encoding.UTF8);
        console.MarkupLineInterpolated($"[green]File created:[/] {testPath}");
        return Unit();
    }
}
