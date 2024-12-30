using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Agent.Infrastructure;
using Ciandt.FlowTools.FlowPair.Agent.Services;
using FluentAssertions;
using FxKit;
using FxKit.Testing.FluentAssertions;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Tests.Agent.Services;

[TestSubject(typeof(ContentDeserializer))]
public class ContentDeserializerTest
{
    public sealed record TestObj(int RiskScore, string Feedback);

    [Fact]
    public void TryDeserializeShouldDeserializeValidObjectJson()
    {
        // Arrange
        var jsonContent = "{\"RiskScore\": 5, \"Feedback\": \"Valid feedback\"}";
        var typeInfo = TestJsonContext.Default.TestObj;

        // Act
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

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
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.Should().BeErr("JSON not found on empty content");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorForUnsupportedJsonKind()
    {
        // Arrange
        var jsonContent = "{\"Key\": \"Value\"}";
        var typeInfo = TestJsonContext.Default.Int32;

        // Act
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.Should().BeErr("JSON value kind not supported: None");
    }

    [Fact]
    public void TryDeserializeShouldReturnErrorForInvalidJsonStructure()
    {
        // Arrange
        var jsonContent = "{\"RiskScore\": 5, \"Feedback\": \"Invalid feedback }";
        var typeInfo = TestJsonContext.Default.TestObj;

        // Act
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

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
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.Should().BeErr("Invalid JSON: '{' not found");
    }

    [Fact]
    public void TryDeserializeShouldHandleNestedJsonContent()
    {
        // Arrange
        var jsonContent = "Invalid content {\"RiskScore\": 10, \"Feedback\": \"Nested JSON\"} More invalid content";
        var typeInfo = TestJsonContext.Default.TestObj;

        // Act
        var result = ContentDeserializer.TryDeserialize(jsonContent, typeInfo);

        // Assert
        result.IsOk.Should().BeTrue();
        var deserialized = result.Unwrap();
        deserialized.RiskScore.Should().Be(10);
        deserialized.Feedback.Should().Be("Nested JSON");
    }
}

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ContentDeserializerTest.TestObj))]
public partial class TestJsonContext : JsonSerializerContext;
