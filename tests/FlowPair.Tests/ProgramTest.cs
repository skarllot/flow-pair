using ConsoleAppFramework;
using FluentAssertions;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Raiqub.LlmTools.FlowPair.Tests;

[TestSubject(typeof(Program))]
public class ProgramTest
{
    [Fact]
    public void HelpShouldReturnAllCommands()
    {
        var console = new TestConsole();
        ConsoleApp.Log = x => console.WriteLine(x);
        ConsoleApp.LogError = x => console.WriteLine(x);

        Program.RunConsoleApp(["--help"]);

        console.Output.Should()
            .Contain("configure")
            .And.Contain("login")
            .And.Contain("review")
            .And.Contain("unittest create")
            .And.Contain("unittest update");
    }

    [Fact]
    public void MainShouldNotThrow()
    {
        var main = () => Program.Main([]);

        main.Should().NotThrow();
    }
}
