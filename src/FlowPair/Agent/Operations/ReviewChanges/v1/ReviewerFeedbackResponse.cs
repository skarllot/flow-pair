using System.Diagnostics.CodeAnalysis;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges.v1;

public sealed record ReviewerFeedbackResponse(
    int RiskScore,
    string RiskDescription,
    string Title,
    string Category,
    string Language,
    string Feedback,
    string Path,
    string LineRange)
{
    [StringSyntax(StringSyntaxAttribute.Json)]
    public const string Schema =
        """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "array",
          "description": "An array of code review feedback items",
          "items": {
            "type": "object",
            "description": "A code review feedback item",
            "properties": {
              "riskScore": {
                "type": "integer",
                "minimum": 0,
                "maximum": 3,
                "description": "The risk level of the feedback based on LOGAF (Likelihood of Occurrence and Gravity of Adverse effects), ranging from 0 (lowest) to 3 (highest)"
              },
              "riskDescription": {
                "type": "string",
                "enum": [
                  "Not important",
                  "Low priority adjustments",
                  "Medium priority adjustments",
                  "High priority adjustments"
                ],
                "description": "A textual description of the risk level"
              },
              "title": {
                "type": "string",
                "minLength": 1,
                "pattern": "^[^\n\r]+$",
                "description": "A brief title or summary of the feedback"
              },
              "category": {
                "type": "string",
                "minLength": 1,
                "pattern": "^[^\n\r]+$",
                "description": "The category or type of the feedback"
              },
              "language": {
                "type": "string",
                "minLength": 1,
                "pattern": "^[A-Za-z+#]+$",
                "description": "The programming language of the code being reviewed"
              },
              "feedback": {
                "type": "string",
                "minLength": 5,
                "description": "Detailed feedback or comments about the code. Use Markdown delimiters (``` or `) for code snippets."
              },
              "path": {
                "type": "string",
                "pattern": "^/.*",
                "description": "The file path of the code being reviewed, starting with a forward slash"
              },
              "lineRange": {
                "type": "string",
                "pattern": "^\\d+(-\\d+)?$",
                "description": "The range of lines affected. Can be a single line number (e.g., '42') or a range (e.g., '42-45')"
              }
            },
            "required": ["riskScore", "riskDescription", "title", "category", "language", "feedback", "path", "lineRange"],
            "additionalProperties": false
          }
        }
        """;
}
