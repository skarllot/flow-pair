using System.Collections.Immutable;
using System.Text;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow.AmazonBedrock.Models.v1;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow.AmazonBedrock;

public interface IBedrockClient : IModelClient;

public sealed class BedrockClient(
    AppJsonContext jsonContext)
    : IBedrockClient
{
    public Result<Message, FlowError> ChatCompletion(
        HttpClient httpClient,
        AllowedModel model,
        ImmutableList<Message> messages)
    {
        var systemMessage = messages
            .Where(m => m.Role == Role.System)
            .Aggregate(new StringBuilder(), (curr, next) => curr.AppendLine(next.Content))
            .ToString();
        var nonSystemMessages = messages
            .Where(m => m.Role != Role.System)
            .ToImmutableList();

        using var responseMessage = httpClient.PostAsJson(
            "/ai-orchestration-api/v1/bedrock/invoke",
            new BedrockChatCompletionRequest([model], nonSystemMessages, systemMessage),
            jsonContext.BedrockChatCompletionRequest);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Failed to retrieve chat completion",
                responseMessage.Content.ReadAsString());
        }

        var response = responseMessage.Content.ReadFromJson(jsonContext.BedrockChatCompletionResponse);
        if (response is null || response.Content.Count == 0)
        {
            return new FlowError(
                responseMessage.StatusCode,
                "Retrieved chat completion is null or empty");
        }

        return response.Content.Select(c => new Message(response.Role, c.Text)).First();
    }
}
