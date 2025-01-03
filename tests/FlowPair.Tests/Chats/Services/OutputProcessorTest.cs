using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.Chats.Services;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(OutputProcessor))]
public class OutputProcessorTest
{
    [Fact]
    public void GetFirstShouldReturnNoneWhenNoMatchingKeyExists()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", "value1" } }),
            CreateChatThread(new Dictionary<string, object> { { "key2", "value2" } })
        ]);

        // Act
        var result = OutputProcessor.GetFirst<string>(chatWorkspace, "nonexistent_key");

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void GetFirstShouldReturnFirstMatchingValueWhenKeyExists()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", "value1" } }),
            CreateChatThread(new Dictionary<string, object> { { "key1", "value2" } })
        ]);

        // Act
        var result = OutputProcessor.GetFirst<string>(chatWorkspace, "key1");

        // Assert
        result.Should().BeSome("value1");
    }

    [Fact]
    public void GetFirstShouldReturnNoneWhenKeyExistsButTypeDoesNotMatch()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", 123 } })
        ]);

        // Act
        var result = OutputProcessor.GetFirst<string>(chatWorkspace, "key1");

        // Assert
        result.Should().BeNone();
    }

    [Fact]
    public void AggregateListsShouldReturnEmptyListWhenNoMatchingKeyExists()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(1, 2, 3) } }),
            CreateChatThread(new Dictionary<string, object> { { "key2", ImmutableList.Create(4, 5, 6) } })
        ]);

        // Act
        var result = OutputProcessor.AggregateLists<int>(chatWorkspace, "nonexistent_key");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AggregateListsShouldAggregateAllMatchingLists()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(1, 2, 3) } }),
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(4, 5, 6) } }),
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(7, 8, 9) } })
        ]);

        // Act
        var result = OutputProcessor.AggregateLists<int>(chatWorkspace, "key1");

        // Assert
        result.Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9);
    }

    [Fact]
    public void AggregateListsShouldIgnoreNonMatchingTypes()
    {
        // Arrange
        var chatWorkspace = CreateChatWorkspace(
        [
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(1, 2, 3) } }),
            CreateChatThread(new Dictionary<string, object> { { "key1", "not a list" } }),
            CreateChatThread(new Dictionary<string, object> { { "key1", ImmutableList.Create(4, 5, 6) } })
        ]);

        // Act
        var result = OutputProcessor.AggregateLists<int>(chatWorkspace, "key1");

        // Assert
        result.Should().Equal(1, 2, 3, 4, 5, 6);
    }

    private static ChatWorkspace CreateChatWorkspace(IEnumerable<ChatThread> chatThreads)
    {
        return new ChatWorkspace(chatThreads.ToImmutableList());
    }

    private static ChatThread CreateChatThread(Dictionary<string, object> outputs)
    {
        return new ChatThread(
            Progress: new ProgressTask(0, "description", 100),
            Model: AllowedModel.Gpt4,
            StopKeyword: "<STOP>",
            Messages: ImmutableList<Message>.Empty,
            MessageParser: null!,
            Outputs: outputs.ToImmutableDictionary());
    }
}
