using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.AnthropicCompleteChat;

public partial interface IAnthropicClient;

[GenerateAutomaticInterface]
public sealed class AnthropicClient(
    FlowHttpClient httpClient,
    AppJsonContext jsonContext)
    : IAnthropicClient
{
    public Result<AnthropicMessage, FlowError> ChatCompletion(
        AllowedAnthropicModels model,
        ImmutableList<AnthropicMessage> messages,
        string? system = null)
    {
        using var responseMessage = httpClient.PostAsJson(
            "/ai-orchestration-api/v1/bedrock/invoke",
            new AnthropicChatCompletionRequest([model], messages, system),
            jsonContext.AnthropicChatCompletionRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to retrieve Anthropic chat completion",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.AnthropicChatCompletionResponse);
        if (response is null || response.Content.Count == 0)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved Anthropic chat completion is null or empty");
        }

        return new AnthropicMessage(response.Role, response.Content);
    }
}
