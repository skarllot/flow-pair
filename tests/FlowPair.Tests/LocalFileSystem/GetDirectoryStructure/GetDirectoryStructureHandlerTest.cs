using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Tests.LocalFileSystem.GetDirectoryStructure;

[TestSubject(typeof(GetDirectoryStructureHandler))]
public class GetDirectoryStructureHandlerTest
{
    private readonly GetDirectoryStructureHandler _handler = new();

    [Fact]
    public void ExecuteEmptyDirectoryReturnsOnlyDirectoryName()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be("TestDir/");
    }

    [Fact]
    public void ExecuteSingleLevelReturnsCorrectStructure()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be(
            """
            TestDir/
            |-- SubDir1/
            |-- SubDir2/
            """);
    }

    [Fact]
    public void ExecuteMultipleLevelReturnsCorrectStructure()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1\SubSubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be(
            """
            TestDir/
            |-- SubDir1/
            |   |-- SubSubDir1/
            |-- SubDir2/
            """);
    }

    [Fact]
    public void ExecuteIgnoresHiddenDirectories()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\.hiddenDir"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be(
            """
            TestDir/
            |-- SubDir1/
            """);
    }

    [Fact]
    public void ExecuteIgnoresListedDirectories()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\node_modules"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\bin"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be(
            """
            TestDir/
            |-- SubDir1/
            """);
    }

    [Fact]
    public void ExecuteHandlesDeepNestedStructure()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PathAnalyzer.Normalize(@"C:\TestDir"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1\SubSubDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir1\SubSubDir1\DeepDir1"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir2"), new MockDirectoryData() },
                { PathAnalyzer.Normalize(@"C:\TestDir\SubDir2\SubSubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathAnalyzer.Normalize(@"C:\TestDir"));

        // Act
        var result = _handler.Execute(directoryInfo);

        // Assert
        result.Should().Be(
            """
            TestDir/
            |-- SubDir1/
            |   |-- SubSubDir1/
            |   |   |-- DeepDir1/
            |-- SubDir2/
            |   |-- SubSubDir2/
            """);
    }
}
