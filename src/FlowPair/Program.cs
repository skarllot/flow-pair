using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.DependencyInjection;
using Raiqub.LlmTools.FlowPair.Settings.Operations.Configure;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair;

public static class Program
{
    private static readonly Style s_errorStyle = new(Color.Red);

    public static void Main(string[] args)
    {
        using var container = new AppContainer();
        var console = container.GetService<IAnsiConsole>();

        ConsoleApp.ServiceProvider = container;
        ConsoleApp.Version = ThisAssembly.AssemblyInformationalVersion;
        ConsoleApp.Log = x => console.WriteLine(x);
        ConsoleApp.LogError = x => console.WriteLine(x, s_errorStyle);

        RunConsoleApp(args);
    }

    public static void RunConsoleApp(string[] args)
    {
        var app = ConsoleApp.Create();
        app.Add<ConfigureCommand>();
        app.Add<LoginCommand>();
        app.Add<ReviewChangesCommand>();
        app.Add<CreateUnitTestCommand>();
        app.Add<UpdateUnitTestCommand>();
        app.Run(args);
    }
}
