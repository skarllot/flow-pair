using System.Collections.Immutable;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAi.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.OpenAi;

public interface IOpenAiClient : IModelClient;

public sealed class OpenAiClient(
    AppJsonContext jsonContext)
    : IOpenAiClient
{
    public Result<Message, FlowError> ChatCompletion(
        HttpClient httpClient,
        AllowedModel model,
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
                "Failed to retrieve chat completion",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.OpenAiChatCompletionResponse);
        if (response is null || response.Choices.Count == 0)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved chat completion is null or empty");
        }

        return response.Choices[0].Message;
    }
}
