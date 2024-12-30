using System.Diagnostics.CodeAnalysis;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;

public sealed record UpdateUnitTestResponse(
    string Content,
    string Description)
{
    [StringSyntax(StringSyntaxAttribute.Json)] public const string Schema =
        """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "description": "Represents a response for creating a unit test",
          "properties": {
            "content": {
              "type": "string",
              "description": "The content of the created unit tests",
              "minLength": 1
            },
            "description": {
              "type": "string",
              "description": "The description of changes made on the unit tests",
              "minLength": 1
            }
          },
          "required": ["content", "description"],
          "additionalProperties": false
        }
        """;
}
