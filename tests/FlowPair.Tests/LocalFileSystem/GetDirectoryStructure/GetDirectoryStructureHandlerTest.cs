using System.IO.Abstractions.TestingHelpers;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using FluentAssertions;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Tests.LocalFileSystem.GetDirectoryStructure;

[TestSubject(typeof(GetDirectoryStructureHandler))]
public class GetDirectoryStructureHandlerTest
{
    private readonly GetDirectoryStructureHandler _handler = new();

    [Fact]
    public void ExecuteEmptyDirectoryReturnsOnlyDirectoryName()
    {
        // Arrange
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { @"C:\TestDir", new MockDirectoryData() } });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
                { @"C:\TestDir", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir2", new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
                { @"C:\TestDir", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1\SubSubDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir2", new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
                { @"C:\TestDir", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1", new MockDirectoryData() },
                { @"C:\TestDir\.hiddenDir", new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
                { @"C:\TestDir", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1", new MockDirectoryData() },
                { @"C:\TestDir\node_modules", new MockDirectoryData() },
                { @"C:\TestDir\bin", new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
                { @"C:\TestDir", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1\SubSubDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir1\SubSubDir1\DeepDir1", new MockDirectoryData() },
                { @"C:\TestDir\SubDir2", new MockDirectoryData() },
                { @"C:\TestDir\SubDir2\SubSubDir2", new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(@"C:\TestDir");

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
