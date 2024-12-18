using System.Collections.Immutable;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.OpenAiCompleteChat;

public partial interface IOpenAiClient;

[GenerateAutomaticInterface]
public sealed class OpenAiClient(
    FlowHttpClient httpClient,
    AppJsonContext jsonContext)
    : IOpenAiClient
{
    public Result<Message, FlowError> ChatCompletion(
        AllowedOpenAiModels model,
        ImmutableList<Message> messages)
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
