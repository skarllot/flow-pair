using System.IO.Abstractions;
using System.Text;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest;

public sealed class UpdateUnitTestCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    IUpdateUnitTestChatDefinition chatDefinition,
    IWorkingDirectoryWalker workingDirectoryWalker,
    IProjectFilesMessageFactory projectFilesMessageFactory,
    IDirectoryStructureMessageFactory directoryStructureMessageFactory,
    ILoginUseCase loginUseCase,
    IChatService chatService)
{
    /// <summary>
    /// Update existing unit test with code changes.
    /// </summary>
    /// <param name="sourceFile">-s, The file path of the code to test.</param>
    /// <param name="testFile">-t, The file path of the existing unit tests.</param>
    [Command("update-unittest")]
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
                let initialMessages = BuildInitialMessages(sourceFileInfo, testFileInfo, rootPath)
                from response in chatService
                    .Run(console.Progress(), AllowedModel.Claude35Sonnet, chatDefinition, initialMessages)
                    .MapErr(HandleChatServiceError)
                let create = RewriteUnitTestFile(testFileInfo, response)
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

    private IEnumerable<Message> BuildInitialMessages(
        IFileInfo sourceFileInfo,
        IFileInfo testFileInfo,
        IDirectoryInfo rootPath)
    {
        yield return projectFilesMessageFactory.CreateWithProjectFilesContent(rootPath);
        yield return directoryStructureMessageFactory.CreateWithRepositoryStructure(rootPath);

        yield return new Message(
            Role.User,
            $"""
             The source file updated content is:
             ```
             {sourceFileInfo.ReadAllText()}
             ```
             """);

        yield return new Message(
            Role.User,
            $"""
             The existing test file content is:
             ```
             {testFileInfo.ReadAllText()}
             ```
             """);
    }

    private Unit RewriteUnitTestFile(IFileInfo testFileInfo, UpdateUnitTestResponse response)
    {
        testFileInfo.WriteAllText(response.Content, Encoding.UTF8);
        console.MarkupLineInterpolated($"[green]Unit tests updated:[/] {response.Description}");
        return Unit();
    }
}
