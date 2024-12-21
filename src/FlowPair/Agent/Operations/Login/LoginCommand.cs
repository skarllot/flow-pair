using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.Login;

public class LoginCommand(
    IAnsiConsole console)
{
    [Command("login")]
    public void Execute()
    {
        console.Write("");
    }
}
