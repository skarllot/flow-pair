using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using static Raiqub.LlmTools.FlowPair.LocalFileSystem.Services.PathAnalyzer;

namespace Raiqub.LlmTools.FlowPair.Tests.LocalFileSystem.Services;

[TestSubject(typeof(WorkingDirectoryWalker))]
public class WorkingDirectoryWalkerTest
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly WorkingDirectoryWalker _walker;

    public WorkingDirectoryWalkerTest()
    {
        _mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize(@"C:\Project\file1.cs"), new MockFileData("") },
                { Normalize(@"C:\Project\file2.cs"), new MockFileData("") },
                { Normalize(@"C:\Project\file.txt"), new MockFileData("") },
                { Normalize(@"C:\Project\SubDir\file3.cs"), new MockFileData("") },
                { Normalize(@"C:\Project\SubDir\file4.txt"), new MockFileData("") },
                { Normalize(@"C:\Project\package.json"), new MockFileData("") },
                { Normalize(@"C:\Project\SubDir\package.json"), new MockFileData("") }
            });
        _walker = new WorkingDirectoryWalker(_mockFileSystem);

        // NOTE: MatchCasing is not supported by the mock file system
        GetFileEnumerationOptions(null) = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        };
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_fileEnumerationOptions")]
    private static extern ref EnumerationOptions GetFileEnumerationOptions(WorkingDirectoryWalker? @this);

    [Fact]
    public void TryFindRepositoryRootWithGitFolderReturnsRepositoryRoot()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize(@"C:\Project\.git"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(Normalize(@"C:\Project\src"));

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(Normalize(@"C:\Project"));
    }

    [Fact]
    public void TryFindRepositoryRootWithoutGitFolderReturnsNone()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(Normalize(@"C:\Project\src"));

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void TryFindRepositoryRootWithNullPathUsesCurrentDirectory()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize(@"C:\CurrentDir\.git"), new MockDirectoryData() },
                { Normalize(@"C:\CurrentDir\file.cs"), new MockFileData("content") }
            },
            Normalize(@"C:\CurrentDir"));
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(null);

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(Normalize(@"C:\CurrentDir"));
    }

    [Fact]
    public void TryFindRepositoryRootWithNestedGitFolderReturnsCorrectRoot()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize(@"C:\Project\.git"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src\SubProject\.git"), new MockDirectoryData() },
                { Normalize(@"C:\Project\src\SubProject\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(Normalize(@"C:\Project\src\SubProject"));

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(Normalize(@"C:\Project\src\SubProject"));
    }

    [Fact]
    public void TryFindRepositoryRootWithNonExistentPathReturnsNone()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(Normalize(@"C:\NonExistentPath"));

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void FindFilesByExtensionShouldReturnFilesWithGivenExtensions()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var extensions = new[] { ".cs", ".txt" };

        // Act
        var result = _walker.FindFilesByExtension(rootDirectory, extensions).ToList();

        // Assert
        result.Should().HaveCount(5);
        result.Select(f => f.Name).Should()
            .BeEquivalentTo("file1.cs", "file2.cs", "file.txt", "file3.cs", "file4.txt");
    }

    [Fact]
    public void FindFilesByExtensionShouldReturnEmptyWhenNoMatchingFiles()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var extensions = new[] { ".js" };

        // Act
        var result = _walker.FindFilesByExtension(rootDirectory, extensions).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindFilesByExtensionShouldSearchRecursively()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var extensions = new[] { ".cs" };

        // Act
        var result = _walker.FindFilesByExtension(rootDirectory, extensions).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(f => f.Name).Should().Equal("file1.cs", "file2.cs", "file3.cs");
    }

    [Fact]
    public void FindFilesByNameShouldReturnFilesWithGivenNames()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var filenames = new[] { "package.json", "file1.cs" };

        // Act
        var result = _walker.FindFilesByName(rootDirectory, filenames).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(f => f.Name).Should().Equal("package.json", "package.json", "file1.cs");
    }

    [Fact]
    public void FindFilesByNameShouldReturnEmptyWhenNoMatchingFiles()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var filenames = new[] { "nonexistent.file" };

        // Act
        var result = _walker.FindFilesByName(rootDirectory, filenames).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindFilesByNameShouldSearchRecursively()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Project"));
        var filenames = new[] { "package.json" };

        // Act
        var result = _walker.FindFilesByName(rootDirectory, filenames).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(f => f.FullName).Should()
            .Equal(Normalize(@"C:\Project\package.json"), Normalize(@"C:\Project\SubDir\package.json"));
    }
}
