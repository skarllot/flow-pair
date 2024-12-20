using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Flow.OpenAiCompleteChat;

public partial interface IOpenAiClient;

[GenerateAutomaticInterface]
public sealed class OpenAiClient(
    FlowHttpClient httpClient,
    AppJsonContext jsonContext)
    : IOpenAiClient
{
    public Result<OpenAiMessage, FlowError> ChatCompletion(
        AllowedOpenAiModels model,
        ImmutableList<OpenAiMessage> messages)
    {
        using var responseMessage = httpClient.PostAsJson(
            "/ai-orchestration-api/v1/openai/chat/completions",
            new OpenAiChatCompletionRequest([model], messages),
            jsonContext.OpenAiChatCompletionRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to retrieve Open AI chat completion",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.OpenAiChatCompletionResponse);
        if (response is null || response.Choices.Count == 0)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved Open AI chat completion is null or empty");
        }

        return response.Choices[0].Message;
    }
}
