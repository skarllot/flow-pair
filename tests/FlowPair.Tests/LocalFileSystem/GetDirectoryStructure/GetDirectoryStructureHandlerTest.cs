using System.IO.Abstractions.TestingHelpers;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using Ciandt.FlowTools.FlowPair.Tests.Mock;
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
            new Dictionary<string, MockFileData>
            {
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1\SubSubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\.hiddenDir"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\node_modules"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\bin"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
                { PathNormalizer.FromWindows(@"C:\TestDir"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1\SubSubDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir1\SubSubDir1\DeepDir1"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir2"), new MockDirectoryData() },
                { PathNormalizer.FromWindows(@"C:\TestDir\SubDir2\SubSubDir2"), new MockDirectoryData() }
            });
        var directoryInfo = fileSystem.DirectoryInfo.New(PathNormalizer.FromWindows(@"C:\TestDir"));

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
