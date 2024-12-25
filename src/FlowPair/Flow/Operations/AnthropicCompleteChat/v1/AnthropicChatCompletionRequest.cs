using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowPair.Flow.Operations.AnthropicCompleteChat.v1;

/// <param name="AllowedModels">The list of models to use for this request.</param>
/// <param name="Messages">The input messages.</param>
/// <param name="System">
/// <para>The system prompt for the request.</para>
/// <para>A system prompt is a way of providing context and instructions to Anthropic Claude, such as specifying a particular goal or role.</para>
/// </param>
/// <param name="AnthropicVersion">The anthropic version.</param>
/// <param name="Temperature">The amount of randomness injected into the response, between 0 and 1.</param>
/// <param name="MaxTokens">The maximum number of tokens to generate before stopping.</param>
/// <summary>
/// See <a href="https://docs.aws.amazon.com/bedrock/latest/userguide/model-parameters-anthropic-claude-messages.html">Anthropic Claude Messages API</a>.
/// </summary>
public sealed record AnthropicChatCompletionRequest(
    [property: JsonPropertyName("allowedModels")] ImmutableList<AllowedAnthropicModels> AllowedModels,
    [property: JsonPropertyName("messages")] ImmutableList<AnthropicMessage> Messages,
    [property: JsonPropertyName("system")] string? System = null,
    [property: JsonPropertyName("anthropic_version")] string AnthropicVersion = "bedrock-2023-05-31",
    [property: JsonPropertyName("temperature")] float? Temperature = null,
    [property: JsonPropertyName("max_tokens")] int MaxTokens = 8192);
