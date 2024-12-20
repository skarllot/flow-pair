using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat.v1;

/// <param name="AllowedModels">The list of models to use for this request.</param>
/// <param name="Messages">A list of messages comprising the conversation so far.</param>
/// <param name="Temperature">
/// <para>What sampling temperature to use, between 0 and 2.</para>
/// <para>Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.</para>
/// <para>We generally recommend altering this or top_p but not both.</para>
/// </param>
/// <param name="TopP">
/// <para>An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass.</para>
/// <para>So 0.1 means only the tokens comprising the top 10% probability mass are considered.</para>
/// <para>We generally recommend altering this or temperature but not both.</para>
/// </param>
/// <param name="Stream">If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available, with the stream terminated by a data: [DONE] message.</param>
/// <param name="Stop">Up to four sequences where the API will stop generating further tokens.</param>
/// <param name="MaxTokens">
/// <para>The maximum number of tokens that can be generated in the chat completion.</para>
/// <para>The total length of input tokens and generated tokens is limited by the model's context length.</para>
/// </param>
/// <param name="User">A unique identifier representing your end-user, which can help Azure OpenAI to monitor and detect abuse.</param>
/// <param name="N">
/// <para>How many chat completion choices to generate for each input message.</para>
/// <para>Note that you'll be charged based on the number of generated tokens across all of the choices. Keep n as 1 to minimize costs.</para>
/// </param>
public sealed record OpenAiChatCompletionRequest(
    [property: JsonPropertyName("allowedModels")] ImmutableList<AllowedOpenAiModels> AllowedModels,
    [property: JsonPropertyName("messages")] ImmutableList<OpenAiMessage> Messages,
    [property: JsonPropertyName("temperature")] float? Temperature = null,
    [property: JsonPropertyName("top_p")] float? TopP = null,
    [property: JsonPropertyName("stream")] bool? Stream = false,
    [property: JsonPropertyName("stop")] ImmutableList<string>? Stop = null,
    [property: JsonPropertyName("max_tokens")] int MaxTokens = 4096,
    [property: JsonPropertyName("user")] string? User = null,
    [property: JsonPropertyName("n")] int? N = 1);
