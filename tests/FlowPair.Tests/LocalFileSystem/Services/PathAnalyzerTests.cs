using FluentAssertions;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Tests.LocalFileSystem.Services;

public class PathAnalyzerTests
{
    [Theory]
    [InlineData("/path/to/file", "/path/to/file")]
    [InlineData("C:/path/to/file", "/path/to/file")]
    [InlineData(@"path\to\file", "path/to/file")]
    [InlineData(@"C:\path\to\file", "/path/to/file")]
    [InlineData(@"\path\to\file", "/path/to/file")]
    [InlineData("", "")]
    [InlineData(@"a:\file.txt", "/file.txt")]
    [InlineData("Z:\\", "/")]
    [InlineData("C:", "/")]
    public void NormalizeOnNonWindowsShouldConvertToUnixStyle(string input, string expected)
    {
        // Arrange
        if (OperatingSystem.IsWindows())
            return;

        // Act
        var result = PathAnalyzer.Normalize(input);

        // Assert
        result.Should().Be(expected);
        result.Should().NotContain("\\");
        if (!string.IsNullOrEmpty(result))
        {
            result.Should().NotStartWith(":");
            result.IndexOf(':').Should().Be(-1);
        }
    }

    [Theory]
    [InlineData("/path/to/file", @"\path\to\file")]
    [InlineData("C:/path/to/file", @"C:\path\to\file")]
    [InlineData(@"path\to\file", @"path\to\file")]
    [InlineData(@"C:\path\to\file", @"C:\path\to\file")]
    [InlineData(@"\path\to\file", @"\path\to\file")]
    [InlineData("", "")]
    [InlineData("a:/file.txt", @"a:\file.txt")]
    [InlineData("Z:\\", "Z:\\")]
    public void NormalizeOnWindowsShouldConvertToWindowsStyle(string input, string expected)
    {
        // Arrange
        if (!OperatingSystem.IsWindows())
            return;

        // Act
        var result = PathAnalyzer.Normalize(input);

        // Assert
        result.Should().Be(expected);
        result.Should().NotContain("/");
    }

    [Theory]
    [InlineData("C:/path/to/file", "/path/to/file")]
    [InlineData(@"D:\path\to\file", "/path/to/file")]
    [InlineData("Z:/some/path", "/some/path")]
    [InlineData(@"a:\file.txt", "/file.txt")]
    public void NormalizeWithDriveLetterOnNonWindowsShouldRemoveDriveLetter(string input, string expected)
    {
        // Arrange
        if (OperatingSystem.IsWindows())
            return;

        // Act
        var result = PathAnalyzer.Normalize(input);

        // Assert
        result.Should().Be(expected);
        result.Should().NotContain(":");
        result.Should().NotContain("\\");
    }

    [Theory]
    [InlineData("C:/path\\to/file")]
    [InlineData(@"/path\to\file")]
    [InlineData("path/to\\file")]
    public void NormalizeWithMixedSeparatorsShouldNormalizeCorrectly(string input)
    {
        // Windows
        if (OperatingSystem.IsWindows())
        {
            var result = PathAnalyzer.Normalize(input);
            result.Should().NotContain("/");
            result.Should().Contain("\\");

            if (input.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
            {
                result.Should().StartWith("C:\\");
            }
        }

        // Non-Windows
        if (!OperatingSystem.IsWindows())
        {
            var result = PathAnalyzer.Normalize(input);
            result.Should().NotContain("\\");
            result.Should().Contain("/");
            result.Should().NotContain(":");
        }
    }

    [Fact]
    public void NormalizeShouldHandleEmptyInput()
    {
        PathAnalyzer.Normalize("").Should().Be("");
    }
}
