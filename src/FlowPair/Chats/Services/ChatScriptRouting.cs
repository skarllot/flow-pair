using Raiqub.LlmTools.FlowPair.Chats.Contracts.v1;

namespace Raiqub.LlmTools.FlowPair.Chats.Services;

public static class ChatScriptRouting
{
    public static Option<IChatScript> FindChatScriptForFile(
        IReadOnlyList<IChatScript> scripts,
        string filePath)
    {
        return scripts
            .Reverse()
            .FirstOrNone(c => c.CanHandleFile(filePath));
    }
}
