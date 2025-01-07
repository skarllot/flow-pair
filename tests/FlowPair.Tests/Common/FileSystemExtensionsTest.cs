using System.IO.Abstractions.TestingHelpers;
using System.Text;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.Common;
using static Raiqub.LlmTools.FlowPair.LocalFileSystem.Services.PathAnalyzer;

namespace Raiqub.LlmTools.FlowPair.Tests.Common;

[TestSubject(typeof(FileSystemExtensions))]
public class FileSystemExtensionsTest
{
    private readonly MockFileSystem _mockFileSystem = new();

    [Fact]
    public void GetRelativePathShouldReturnCorrectRelativePath()
    {
        // Arrange
        var directory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Base"));
        var path = Normalize(@"C:\Base\SubDir\File.txt");

        // Act
        var result = directory.GetRelativePath(path);

        // Assert
        result.Should().Be(Normalize("SubDir\\File.txt"));
    }

    [Fact]
    public void NewFileShouldReturnCorrectFileInfo()
    {
        // Arrange
        var directory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Base"));
        const string fileName = "File.txt";

        // Act
        var result = directory.NewFile(fileName);

        // Assert
        result.FullName.Should().Be(Normalize(@"C:\Base\File.txt"));
    }

    [Fact]
    public void NewFileIfExistsShouldReturnSomeWhenFileExists()
    {
        // Arrange
        var directory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Base"));
        const string fileName = "ExistingFile.txt";
        _mockFileSystem.AddFile(Normalize(@"C:\Base\ExistingFile.txt"), new MockFileData("Content"));

        // Act
        var result = directory.NewFileIfExists(fileName);

        // Assert
        result.Should().BeSome()
            .FullName.Should().Be(Normalize(@"C:\Base\ExistingFile.txt"));
    }

    [Fact]
    public void NewFileIfExistsShouldReturnNoneWhenFileDoesNotExist()
    {
        // Arrange
        var directory = _mockFileSystem.DirectoryInfo.New(Normalize(@"C:\Base"));
        const string fileName = "NonExistentFile.txt";

        // Act
        var result = directory.NewFileIfExists(fileName);

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void WriteAllTextShouldWriteContentToFile()
    {
        // Arrange
        var file = _mockFileSystem.FileInfo.New(Normalize(@"C:\Test.txt"));
        const string content = "Test content";

        // Act
        file.WriteAllText(content, Encoding.UTF8);

        // Assert
        _mockFileSystem.GetFile(Normalize(@"C:\Test.txt")).TextContents.Should().Be(content);
    }

    [Fact]
    public void ReadAllTextShouldReturnFileContent()
    {
        // Arrange
        const string content =
            """
            Test content
            Other line
            """;

        _mockFileSystem.AddFile(Normalize(@"C:\Test.txt"), new MockFileData(content));
        var file = _mockFileSystem.FileInfo.New(Normalize(@"C:\Test.txt"));

        // Act
        var result = file.ReadAllText();

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void ReadAllTextToShouldAppendContentToStringBuilder()
    {
        // Arrange
        const string content =
            """
            Test content
            Other line
            """;

        _mockFileSystem.AddFile(Normalize(@"C:\Test.txt"), new MockFileData(content));
        var file = _mockFileSystem.FileInfo.New(Normalize(@"C:\Test.txt"));
        var sb = new StringBuilder();

        // Act
        file.ReadAllTextTo(sb);

        // Assert
        sb.ToString().Should().Be(content);
    }

    [Fact]
    public void ReadAllTextToShouldHandleLargeFile()
    {
        // Arrange
        var content = new string('A', 10000); // Large content
        _mockFileSystem.AddFile(Normalize(@"C:\LargeFile.txt"), new MockFileData(content));
        var file = _mockFileSystem.FileInfo.New(Normalize(@"C:\LargeFile.txt"));
        var sb = new StringBuilder();

        // Act
        file.ReadAllTextTo(sb);

        // Assert
        sb.ToString().Should().Be(content);
    }
}
