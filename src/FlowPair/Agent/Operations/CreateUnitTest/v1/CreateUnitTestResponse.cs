namespace Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest.v1;

public sealed record CreateUnitTestResponse(
    string FilePath,
    string Content)
{
    public const string Schema =
        """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "description": "Represents a response for creating a unit test",
          "properties": {
            "filePath": {
              "type": "string",
              "description": "The file path of the created unit test (relative to the repository root directory)",
              "minLength": 1
            },
            "content": {
              "type": "string",
              "description": "The content of the created unit test",
              "minLength": 1
            }
          },
          "required": ["filePath", "content"],
          "additionalProperties": false
        }
        """;
}
