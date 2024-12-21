using System.IO.Abstractions;
using Jab;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.DependencyInjection;

[ServiceProviderModule]
[Singleton(typeof(TimeProvider), Factory = nameof(GetTimeProvider))]
[Singleton(typeof(IFileSystem), typeof(FileSystem))]
[Singleton(typeof(IAnsiConsole), Factory = nameof(CreateAnsiConsole))]
public interface IExternalModule
{
    static IAnsiConsole CreateAnsiConsole() => AnsiConsole.Create(
        new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = new AnsiConsoleOutput(Console.Out),
        });

    static TimeProvider GetTimeProvider() => TimeProvider.System;
}
