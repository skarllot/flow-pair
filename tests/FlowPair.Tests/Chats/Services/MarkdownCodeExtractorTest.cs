using System.Collections.Immutable;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Chats.Services;

namespace Raiqub.LlmTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(MarkdownCodeExtractor))]
public class MarkdownCodeExtractorTest
{
    [Fact]
    public void TryExtractWithValidMarkdownShouldReturnCodeSnippets()
    {
        // Arrange
        const string markdown =
            """
            Some text

            ```csharp
            var x = 5;
            ```

            More text

            ```python
            def hello():
                print('Hello, World!')
            ```
            """;

        // Act
        var result = MarkdownCodeExtractor.TryExtract(markdown);

        // Assert
        result.Should().BeOk().Should().BeEquivalentTo(
            ImmutableList.Create(
                new CodeSnippet("var x = 5;", "csharp"),
                new CodeSnippet($"def hello():{Environment.NewLine}    print('Hello, World!')", "python")
            )
        );
    }

    [Fact]
    public void TryExtractWithEmptyContentShouldReturnError()
    {
        // Arrange
        const string markdown = "";

        // Act
        var result = MarkdownCodeExtractor.TryExtract(markdown);

        // Assert
        result.Should().BeErr("Code not found on empty content");
    }

    [Fact]
    public void TryExtractWithNoCodeBlocksShouldReturnEmptyList()
    {
        // Arrange
        const string markdown = "This is just regular text without any code blocks.";

        // Act
        var result = MarkdownCodeExtractor.TryExtract(markdown);

        // Assert
        result.Should().BeOk().Should().BeEmpty();
    }

    [Theory]
    [InlineData("javascript")]
    [InlineData("")]
    public void TryExtractSingleWithValidSingleCodeBlockShouldReturnCodeSnippet(string language)
    {
        // Arrange
        var markdown =
            $"""
             Some text

             ```{language}
             console.log('Hello, World!');
             ```

             More text
             """;

        // Act
        var result = MarkdownCodeExtractor.TryExtractSingle(markdown);

        // Assert
        result.Should().BeOk().Should().BeEquivalentTo(
            new CodeSnippet("console.log('Hello, World!');", language)
        );
    }

    [Fact]
    public void TryExtractSingleWithEmptyContentShouldReturnError()
    {
        // Arrange
        const string markdown = "";

        // Act
        var result = MarkdownCodeExtractor.TryExtractSingle(markdown);

        // Assert
        result.Should().BeErr("Code not found on empty content");
    }

    [Fact]
    public void TryExtractSingleWithNoCodeBlocksShouldReturnError()
    {
        // Arrange
        const string markdown = "This is just regular text without any code blocks.";

        // Act
        var result = MarkdownCodeExtractor.TryExtractSingle(markdown);

        // Assert
        result.Should().BeErr("No code block found");
    }

    [Fact]
    public void TryExtractSingleWithMultipleCodeBlocksShouldReturnError()
    {
        // Arrange
        const string markdown =
            """
            ```csharp
            var x = 5;
            ```

            ```python
            def hello():
                print('Hello, World!')
            ```
            """;

        // Act
        var result = MarkdownCodeExtractor.TryExtractSingle(markdown);

        // Assert
        result.Should().BeErr("More than one code block found");
    }
}
