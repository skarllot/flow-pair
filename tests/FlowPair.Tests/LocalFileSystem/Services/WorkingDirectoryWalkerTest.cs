using System.IO.Abstractions.TestingHelpers;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Tests.LocalFileSystem.Services;

[TestSubject(typeof(WorkingDirectoryWalker))]
public class WorkingDirectoryWalkerTest
{
    [Fact]
    public void TryFindRepositoryRootWithGitFolderReturnsRepositoryRoot()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\Project\.git"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(PathAnalyzer.Normalize(@"C:\Project\src"));

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(PathAnalyzer.Normalize(@"C:\Project"));
    }

    [Fact]
    public void TryFindRepositoryRootWithoutGitFolderReturnsNone()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(PathAnalyzer.Normalize(@"C:\Project\src"));

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
                { PathAnalyzer.Normalize(@"C:\CurrentDir\.git"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\CurrentDir\file.cs"), new MockFileData("content") }
            },
            PathAnalyzer.Normalize(@"C:\CurrentDir"));
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(null);

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(PathAnalyzer.Normalize(@"C:\CurrentDir"));
    }

    [Fact]
    public void TryFindRepositoryRootWithNestedGitFolderReturnsCorrectRoot()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\Project\.git"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src\SubProject\.git"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\Project\src\SubProject\file.cs"), new MockFileData("content") }
            });
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(PathAnalyzer.Normalize(@"C:\Project\src\SubProject"));

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(PathAnalyzer.Normalize(@"C:\Project\src\SubProject"));
    }

    [Fact]
    public void TryFindRepositoryRootWithNonExistentPathReturnsNone()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        var walker = new WorkingDirectoryWalker(fileSystem);

        // Act
        var result = walker.TryFindRepositoryRoot(PathAnalyzer.Normalize(@"C:\NonExistentPath"));

        // Assert
        result.Should().BeNone();
    }
}
