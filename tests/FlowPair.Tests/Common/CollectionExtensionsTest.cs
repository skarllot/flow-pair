using Ciandt.FlowTools.FlowPair.Common;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using CollectionExtensions = Ciandt.FlowTools.FlowPair.Common.CollectionExtensions;

namespace Ciandt.FlowTools.FlowPair.Tests.Common;

[TestSubject(typeof(CollectionExtensions))]
public class CollectionExtensionsTest
{
    [Fact]
    public void AggregateToStringLinesEmptyCollectionReturnsEmptyString()
    {
        // Arrange
        List<string> collection = [];

        // Act
        var result = collection.AggregateToStringLines(x => x);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AggregateToStringLinesSingleElementReturnsElementWithoutNewLines()
    {
        // Arrange
        List<string> collection = ["Test"];

        // Act
        var result = collection.AggregateToStringLines(x => x);

        // Assert
        result.Should().Be("Test");
    }

    [Fact]
    public void AggregateToStringLinesMultipleElementsReturnsElementsSeparatedByDoubleNewLines()
    {
        // Arrange
        List<string> collection = ["Test1", "Test2", "Test3"];
        var expectedResult =
            $"Test1{Environment.NewLine}{Environment.NewLine}" +
            $"Test2{Environment.NewLine}{Environment.NewLine}" +
            $"Test3";

        // Act
        var result = collection.AggregateToStringLines(x => x);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void AggregateToStringLinesCustomSelectorAppliesSelectorToElements()
    {
        // Arrange
        List<int> collection = [1, 2, 3];
        var expectedResult =
            $"Number: 1{Environment.NewLine}{Environment.NewLine}" +
            $"Number: 2{Environment.NewLine}{Environment.NewLine}" +
            $"Number: 3";

        // Act
        var result = collection.AggregateToStringLines(x => $"Number: {x}");

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void TrySingleEmptyCollectionReturnsEmptyProblem()
    {
        // Arrange
        List<string> collection = [];

        // Act
        var result = collection.TrySingle();

        // Assert
        result.Should().BeErr(SingleElementProblem.Empty);
    }

    [Fact]
    public void TrySingleSingleElementReturnsElement()
    {
        // Arrange
        List<string> collection = ["Test"];

        // Act
        var result = collection.TrySingle();

        // Assert
        result.Should().BeOk("Test");
    }

    [Fact]
    public void TrySingleMultipleElementsReturnsMoreThanOneElementProblem()
    {
        // Arrange
        List<string> collection = ["Test1", "Test2"];

        // Act
        var result = collection.TrySingle();

        // Assert
        result.Should().BeErr(SingleElementProblem.MoreThanOneElement);
    }

    [Fact]
    public void TrySingleNonListEnumerableHandlesCorrectly()
    {
        // Arrange
        IEnumerable<string> YieldOneElement()
        {
            yield return "Test";
        }

        // Act
        var result = YieldOneElement().TrySingle();

        // Assert
        result.Should().BeOk("Test");
    }

    [Fact]
    public void TrySingleNonListEnumerableMultipleElementsReturnsMoreThanOneElementProblem()
    {
        // Arrange
        IEnumerable<string> YieldMultipleElements()
        {
            yield return "Test1";
            yield return "Test2";
        }

        // Act
        var result = YieldMultipleElements().TrySingle();

        // Assert
        result.Should().BeErr(SingleElementProblem.MoreThanOneElement);
    }
}
