namespace Ciandt.FlowTools.FlowPair.Chats.Services;

public interface IMessageParser
{
    Result<object, string> Parse(string key, string input);
}
