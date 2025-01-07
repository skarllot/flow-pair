namespace Raiqub.LlmTools.FlowPair.Chats.Models;

public sealed record CodeSnippet(
    string Content,
    string? Language = null);
