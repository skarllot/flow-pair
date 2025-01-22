using FluentAssertions;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;
using System.Collections.Immutable;

namespace Raiqub.LlmTools.FlowPair.Tests.LocalFileSystem.Services;

public class KnownFileExtensionTests
{
    [Fact]
    public void UnitTestableShouldHaveCorrectContentAndProperties()
    {
        // Arrange
        var expectedExtensions = new[]
        {
            ".py", ".pyw", ".pyx", ".js", ".jsx", ".mjs", ".cjs", ".java", ".cs",
            ".cpp", ".cxx", ".cc", ".c++", ".php", ".rb", ".swift", ".r", ".kt",
            ".ts", ".tsx", ".go", ".rs", ".scala", ".dart", ".pl", ".pm", ".m",
            ".bas", ".cls",
        };

        // Act
        var unitTestable = KnownFileExtension.UnitTestable;

        // Assert
        unitTestable.Should().BeOfType<ImmutableArray<string>>();
        expectedExtensions.Should().BeSubsetOf(unitTestable);
        unitTestable.Should().HaveCountGreaterOrEqualTo(expectedExtensions.Length);
        unitTestable.Should().Contain(".py");
        unitTestable.Should().Contain(".cls");
    }

    [Fact]
    public void NotUnitTestableShouldHaveCorrectContentAndProperties()
    {
        // Arrange
        var expectedExtensions = new[]
        {
            ".pxd", ".pxi", ".csx", ".hpp", ".hxx", ".h", ".hh", ".h++", ".phtml",
            ".phps", ".rbw", ".rake", ".sql", ".kts", ".sc", ".t", ".pod", ".frm",
            ".sh", ".bash", ".zsh", ".ksh", ".csh", ".tcsh", ".fish"
        };

        // Act
        var notUnitTestable = KnownFileExtension.NotUnitTestable;

        // Assert
        notUnitTestable.Should().BeOfType<ImmutableArray<string>>();
        expectedExtensions.Should().BeSubsetOf(notUnitTestable);
        notUnitTestable.Should().HaveCountGreaterOrEqualTo(expectedExtensions.Length);
        notUnitTestable.Should().Contain(".pxd");
        notUnitTestable.Should().Contain(".fish");
    }

    [Fact]
    public void AllShouldHaveCorrectContentAndProperties()
    {
        // Arrange
        var expectedAll = KnownFileExtension.UnitTestable.AddRange(KnownFileExtension.NotUnitTestable);

        // Act
        var all = KnownFileExtension.All;

        // Assert
        all.Should().BeOfType<ImmutableArray<string>>();
        all.Should().BeEquivalentTo(expectedAll, options => options.WithStrictOrdering());
        all.Should().HaveCount(KnownFileExtension.UnitTestable.Length + KnownFileExtension.NotUnitTestable.Length);
        all.First().Should().Be(KnownFileExtension.UnitTestable.First());
        all.Last().Should().Be(KnownFileExtension.NotUnitTestable.Last());
        all.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void UnitTestableAndNotUnitTestableShouldNotOverlap()
    {
        // Act
        var unitTestable = KnownFileExtension.UnitTestable;
        var notUnitTestable = KnownFileExtension.NotUnitTestable;

        // Assert
        unitTestable.Should().NotIntersectWith(notUnitTestable);
    }

    [Theory]
    [InlineData(".cs")]
    [InlineData(".py")]
    [InlineData(".java")]
    [InlineData(".cpp")]
    [InlineData(".rb")]
    public void UnitTestableShouldContainCommonProgrammingLanguages(string extension)
    {
        // Act
        var unitTestable = KnownFileExtension.UnitTestable;

        // Assert
        unitTestable.Should().Contain(extension);
    }

    [Theory]
    [InlineData(".sh")]
    [InlineData(".sql")]
    [InlineData(".h")]
    public void NotUnitTestableShouldContainSpecificNonTestableExtensions(string extension)
    {
        // Act
        var notUnitTestable = KnownFileExtension.NotUnitTestable;

        // Assert
        notUnitTestable.Should().Contain(extension);
    }

    [Fact]
    public void AllExtensionsShouldStartWithDotAndBeLowerCase()
    {
        // Act
        var allExtensions = KnownFileExtension.All;

        // Assert
        allExtensions.Should().AllSatisfy(ext =>
        {
            ext.Should().StartWith(".");
            ext.Should().Be(ext.ToLowerInvariant());
        });
    }
}
