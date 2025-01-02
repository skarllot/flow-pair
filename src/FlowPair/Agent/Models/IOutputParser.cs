namespace Ciandt.FlowTools.FlowPair.Agent.Models;

public interface IOutputParser
{
    Result<object, string> Parse(string key, string input);
}
