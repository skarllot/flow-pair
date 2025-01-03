using System.IO.Abstractions;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Services;

[TestSubject(typeof(DirectoryStructureMessageFactory))]
public class DirectoryStructureMessageFactoryTest
{
    private readonly IGetDirectoryStructureHandler _getDirectoryStructureHandler;
    private readonly DirectoryStructureMessageFactory _factory;

    public DirectoryStructureMessageFactoryTest()
    {
        _getDirectoryStructureHandler = Substitute.For<IGetDirectoryStructureHandler>();
        _factory = new DirectoryStructureMessageFactory(_getDirectoryStructureHandler);
    }

    [Fact]
    public void CreateWithRepositoryStructureShouldReturnCorrectMessage()
    {
        // Arrange
        var mockDirectory = Substitute.For<IDirectoryInfo>();
        const string expectedStructure = "mock/directory/structure";
        _getDirectoryStructureHandler
            .Execute(mockDirectory)
            .Returns(expectedStructure);

        // Act
        var result = _factory.CreateWithRepositoryStructure(mockDirectory);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(SenderRole.User);
        result.Content.Should().Contain("The repository has the following directory structure:");
        result.Content.Should().Contain("```");
        result.Content.Should().Contain(expectedStructure);
        _getDirectoryStructureHandler.Received(1).Execute(mockDirectory);
    }

    [Fact]
    public void CreateWithRepositoryStructureShouldHandleEmptyStructure()
    {
        // Arrange
        var mockDirectory = Substitute.For<IDirectoryInfo>();
        _getDirectoryStructureHandler
            .Execute(mockDirectory)
            .Returns(string.Empty);

        // Act
        var result = _factory.CreateWithRepositoryStructure(mockDirectory);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(SenderRole.User);
        result.Content.Should().Contain("The repository has the following directory structure:");
        result.Content.Should().Contain("```");
    }
}