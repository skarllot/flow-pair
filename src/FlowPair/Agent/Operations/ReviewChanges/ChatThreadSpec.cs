using System.Collections.Immutable;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public static class ChatThreadSpec
{
    public static bool IsClosed(ImmutableList<Message> thread, string stopKeyword) =>
        thread[^1].Role == Role.Assistant &&
        thread[^1].Content.Contains(stopKeyword, StringComparison.Ordinal);
}
