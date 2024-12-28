using System.Collections.Frozen;
using System.IO.Abstractions;
using System.Text;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;

public sealed class CreateUnitTestCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    IWorkingDirectoryWalker workingDirectoryWalker,
    IGetDirectoryStructureHandler getDirectoryStructureHandler,
    ILoginUseCase loginUseCase)
{
    private static string[] s_projectExtensions =
    [
        ".csproj", ".slnx", // C#
        ".Rproj", // R
        ".xcodeproj", ".xcworkspace", // Swift
        ".project", // Java (Eclipse)
        ".workspace", // C++ (CodeBlocks)
        ".idea", // Kotlin, Scala (IntelliJ IDEA)
        ".prj", // MATLAB
    ];

    private static FrozenSet<string> s_projectFiles = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        [
            "Directory.Packages.props", "Directory.Build.props", "Directory.Build.targets", // C#
            "pom.xml", "build.gradle", // Java (Maven, Gradle)
            "pyproject.toml", "setup.py", // Python
            "package.json", // JavaScript
            "CMakeLists.txt", "Makefile", // C++
            "composer.json", // PHP
            "Gemfile", // Ruby
            "Package.swift", // Swift
            "DESCRIPTION", // R
            "build.gradle.kts", // Kotlin
            "tsconfig.json", // TypeScript
            "go.mod", // Go (Golang)
            "Cargo.toml", // Rust
            "build.sbt", // Scala
            "pubspec.yaml", // Dart
            "Makefile.PL", "dist.ini", // Perl
        ]);

    /// <summary>
    /// Create unit test for the code on the file.
    /// </summary>
    /// <param name="filePath">-f, The file path of the code to test.</param>
    [Command("create-unittest")]
    public int Execute(
        string filePath)
    {
        var fileInfo = fileSystem.FileInfo.New(filePath);
        if (!fileInfo.Exists)
        {
            console.MarkupLine("[red]Error:[/] The specified file does not exist.");
            return 1;
        }

        return (from rootPath in workingDirectoryWalker.TryFindRepositoryRoot(filePath).OkOr(2)
                    .MapErr(HandleFindRepositoryRootError)
                from session in loginUseCase.Execute(isBackground: true)
                    .UnwrapErrOr(0)
                    .Ensure(n => n == 0, 1)
                let initialMessages = BuildInitialMessages(fileInfo, rootPath)
                select 0)
            .UnwrapEither();
    }

    private int HandleFindRepositoryRootError(int errorCode)
    {
        console.MarkupLine("[red]Error:[/] Could not locate Git repository.");
        return errorCode;
    }

    private IEnumerable<Message> BuildInitialMessages(IFileInfo fileInfo, IDirectoryInfo rootPath)
    {
        yield return new Message(
            Role.User,
            workingDirectoryWalker
                .FindFilesByExtension(rootPath, s_projectExtensions)
                .Concat(workingDirectoryWalker.FindFilesByName(rootPath, s_projectFiles))
                .Aggregate(new StringBuilder(), (curr, next) => AggregateFileContent(curr, next, rootPath))
                .ToString());

        yield return new Message(
            Role.User,
            $"""
             The repository has the following directory structure:
             {getDirectoryStructureHandler.Execute(rootPath)}
             """);

        yield return new Message(
            Role.User,
            $"""
             Create unit tests for the following code:
             {fileInfo.ReadAllText()}
             """);
    }

    private static StringBuilder AggregateFileContent(
        StringBuilder sb,
        IFileInfo fileInfo,
        IDirectoryInfo rootDirectory)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        var rootPathLength = rootDirectory.FullName.Length;

        sb.Append("File: ");
        sb.Append(fileInfo.FullName.AsSpan()[rootPathLength..]);
        sb.AppendLine();
        sb.AppendLine("```");
        fileInfo.ReadAllTextTo(sb);
        sb.AppendLine("```");
        return sb;
    }
}
