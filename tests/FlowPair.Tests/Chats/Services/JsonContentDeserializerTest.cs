using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Chats.Services;
using FluentAssertions;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Tests.Chats.Services;

[TestSubject(typeof(JsonContentDeserializer))]
public class JsonContentDeserializerTest
{
    public sealed record TestObj(int RiskScore, string Feedback);

    [Fact]
    public void TryDeserializeShouldDeserializeValidObjectJson()
    {
        // Arrange
        var jsonContent = "{\"RiskScore\": 5, \"Feedback\": \"Valid feedback\"}";
        var typeInfo = JsonSerializerOptions.Web.GetTypeInfo(typeof(TestObj));

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, (JsonTypeInfo<TestObj>)typeInfo);

        // Assert
        var testObj = result.Should().BeOk();
        testObj.RiskScore.Should().Be(5);
        testObj.Feedback.Should().Be("Valid feedback");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorForEmptyContent()
    {
        // Arrange
        var jsonContent = "";
        var typeInfo = AgentJsonContext.Default.ReviewerFeedbackResponse;

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.Should().BeErr("JSON not found on empty content");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorForUnsupportedJsonKind()
    {
        // Arrange
        var jsonContent = "{\"Key\": \"Value\"}";
        var typeInfo = JsonSerializerOptions.Web.GetTypeInfo(typeof(int));

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, (JsonTypeInfo<int>)typeInfo);

        // Assert
        result.Should().BeErr("JSON value kind not supported: None");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorForInvalidJsonStructure()
    {
        // Arrange
        var jsonContent = "{\"RiskScore\": 5, \"Feedback\": \"Invalid feedback }";
        var typeInfo = JsonSerializerOptions.Web.GetTypeInfo(typeof(TestObj));

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, (JsonTypeInfo<TestObj>)typeInfo);

        // Assert
        result.Should().BeErr()
            .Should().StartWith("Expected end of string");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorWhenJsonNotFoundWithinContent()
    {
        // Arrange
        var jsonContent = "Invalid content without JSON";
        var typeInfo = AgentJsonContext.Default.ReviewerFeedbackResponse;

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.Should().BeErr("Invalid JSON: '{' not found");
    }

    [Fact]
    public void TryDeserializeShouldHandleNestedJsonContent()
    {
        // Arrange
        var jsonContent = "Invalid content {\"RiskScore\": 10, \"Feedback\": \"Nested JSON\"} More invalid content";
        var typeInfo = JsonSerializerOptions.Web.GetTypeInfo(typeof(TestObj));

        // Act
        var result = JsonContentDeserializer.TryDeserialize(jsonContent, (JsonTypeInfo<TestObj>)typeInfo);

        // Assert
        result.IsOk.Should().BeTrue();
        var deserialized = result.Unwrap();
        deserialized.RiskScore.Should().Be(10);
        deserialized.Feedback.Should().Be("Nested JSON");
    }
}
