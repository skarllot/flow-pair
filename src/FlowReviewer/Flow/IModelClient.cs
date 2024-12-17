using System.Collections.Immutable;
using Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

namespace Ciandt.FlowTools.FlowReviewer.Flow;

public interface IModelClient
{
    Result<Message, FlowError> ChatCompletion(
        HttpClient httpClient,
        AllowedModel model,
        ImmutableList<Message> messages);
}
