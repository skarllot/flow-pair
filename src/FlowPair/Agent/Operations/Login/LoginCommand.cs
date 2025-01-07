using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Common;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.Login;

public class LoginCommand(
    ILoginUseCase loginUseCase)
{
    /// <summary>
    /// Sign in to the Flow.
    /// </summary>
    [Command("login")]
    public int Execute()
    {
        return loginUseCase.Execute(isBackground: false)
            .UnwrapErrOr(0);
    }
}
