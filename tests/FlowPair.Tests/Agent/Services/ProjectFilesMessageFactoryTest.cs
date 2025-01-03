using System.IO.Abstractions.TestingHelpers;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using static Ciandt.FlowTools.FlowPair.LocalFileSystem.Services.PathAnalyzer;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Services;

[TestSubject(typeof(ProjectFilesMessageFactory))]
public class ProjectFilesMessageFactoryTest
{
    private readonly IWorkingDirectoryWalker _mockWorkingDirectoryWalker;
    private readonly ProjectFilesMessageFactory _factory;
    private readonly MockFileSystem _mockFileSystem;
    private const string RootPath = @"c:\project";

    public ProjectFilesMessageFactoryTest()
    {
        _mockWorkingDirectoryWalker = Substitute.For<IWorkingDirectoryWalker>();
        _mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Normalize($@"{RootPath}\project.csproj"), new MockFileData("<Project></Project>") },
                { Normalize($@"{RootPath}\package.json"), new MockFileData("{ \"name\": \"test-project\" }") },
                { Normalize($@"{RootPath}\src\Program.cs"), new MockFileData("public class Program {}") }
            });
        _factory = new ProjectFilesMessageFactory(_mockWorkingDirectoryWalker);
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldReturnCorrectMessage()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));
        _mockWorkingDirectoryWalker
            .FindFilesByExtension(rootDirectory, Arg.Any<IEnumerable<string>>())
            .Returns([_mockFileSystem.FileInfo.New(Normalize($@"{RootPath}\project.csproj"))]);
        _mockWorkingDirectoryWalker
            .FindFilesByName(rootDirectory, Arg.Any<IEnumerable<string>>())
            .Returns([_mockFileSystem.FileInfo.New(Normalize($@"{RootPath}\package.json"))]);

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(SenderRole.User);
        result.Content.Should().Contain("File: project.csproj");
        result.Content.Should().Contain("<Project></Project>");
        result.Content.Should().Contain("File: package.json");
        result.Content.Should().Contain("{ \"name\": \"test-project\" }");
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldUseProvidedExtensions()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));
        var customExtensions = new[] { ".cs" };

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory, extensions: customExtensions);

        // Assert
        result.Should().NotBeNull();
        _mockWorkingDirectoryWalker.Received(1).FindFilesByExtension(rootDirectory, customExtensions);
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldUseProvidedFilenames()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));
        var customFilenames = new[] { "Program.cs" };

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory, filenames: customFilenames);

        // Assert
        result.Should().NotBeNull();
        _mockWorkingDirectoryWalker.Received(1).FindFilesByName(rootDirectory, customFilenames);
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldUseDefaultExtensionsAndFilenames()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory);

        // Assert
        result.Should().NotBeNull();
        _mockWorkingDirectoryWalker.Received(1).FindFilesByExtension(rootDirectory, FileNaming.ProjectExtensions);
        _mockWorkingDirectoryWalker.Received(1).FindFilesByName(rootDirectory, FileNaming.ProjectFiles);
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldHandleAllDefaultExtensions()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory);

        // Assert
        result.Should().NotBeNull();
        _mockWorkingDirectoryWalker.Received(1).FindFilesByExtension(
            rootDirectory,
            Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(FileNaming.ProjectExtensions)));
    }

    [Fact]
    public void CreateWithProjectFilesContentShouldHandleAllDefaultFilenames()
    {
        // Arrange
        var rootDirectory = _mockFileSystem.DirectoryInfo.New(Normalize(RootPath));

        // Act
        var result = _factory.CreateWithProjectFilesContent(rootDirectory);

        // Assert
        result.Should().NotBeNull();
        _mockWorkingDirectoryWalker.Received(1).FindFilesByName(
            rootDirectory,
            Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(FileNaming.ProjectFiles)));
    }
}
