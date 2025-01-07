namespace Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;

public sealed record FilePathResponse(
    string FilePath)
{
    public const string Schema =
        """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "description": "Represents a response for creating a unit test file",
          "properties": {
            "filePath": {
              "type": "string",
              "description": "The file path of the created unit test (relative to the repository root directory)",
              "minLength": 1
            }
          },
          "required": ["filePath"],
          "additionalProperties": false
        }
        """;
}
