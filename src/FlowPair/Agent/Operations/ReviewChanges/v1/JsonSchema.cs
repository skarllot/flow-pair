using System.Diagnostics.CodeAnalysis;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges.v1;

public static class JsonSchema
{
    [StringSyntax(StringSyntaxAttribute.Json)]
    public const string FeedbackJsonSchema =
        """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "riskScore": {
                "type": "integer",
                "minimum": 0,
                "maximum": 3
              },
              "riskDescription": {
                "type": "string",
                "enum": [
                  "Not important",
                  "Low priority adjustments",
                  "Medium priority adjustments",
                  "High priority adjustments"
                ]
              },
              "feedback": {
                "type": "string",
                "minLength": 5
              },
              "path": {
                "type": "string",
                "pattern": "^/.*"
              },
              "lineRange": {
                "type": "string",
                "pattern": "^\\d+-\\d+$"
              }
            },
            "required": ["riskScore", "riskDescription", "feedback", "path", "lineRange"],
            "additionalProperties": false
          }
        }
        """;
}
