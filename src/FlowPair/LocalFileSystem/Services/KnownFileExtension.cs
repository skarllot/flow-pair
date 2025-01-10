using System.Collections.Immutable;

namespace Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

public static class KnownFileExtension
{
    public static ImmutableArray<string> UnitTestable { get; } =
    [
        /* Python          */".py", ".pyw", ".pyx",
        /* JavaScript      */".js", ".jsx", ".mjs", ".cjs",
        /* Java            */".java",
        /* C#              */".cs",
        /* C++             */".cpp", ".cxx", ".cc", ".c++",
        /* PHP             */".php",
        /* Ruby            */".rb",
        /* Swift           */".swift",
        /* R               */".r",
        /* Kotlin          */".kt",
        /* TypeScript      */".ts", ".tsx",
        /* Go (Golang)     */".go",
        /* Rust            */".rs",
        /* Scala           */".scala",
        /* Dart            */".dart",
        /* Perl            */".pl", ".pm",
        /* MATLAB          */".m",
        /* VBA             */".bas", ".cls",
    ];

    public static ImmutableArray<string> NotUnitTestable { get; } =
    [
        /* Python          */".pxd", ".pxi",
        /* C#              */".csx",
        /* C++             */".hpp", ".hxx", ".h", ".hh", ".h++",
        /* PHP             */".phtml", ".phps",
        /* Ruby            */".rbw", ".rake",
        /* SQL             */".sql",
        /* Kotlin          */".kts",
        /* Scala           */".sc",
        /* Perl            */".t", ".pod",
        /* VBA             */".frm",
        /* Shell Scripting */".sh", ".bash", ".zsh", ".ksh", ".csh", ".tcsh", ".fish",
    ];

    public static ImmutableArray<string> All { get; } = [..UnitTestable, ..NotUnitTestable];
}
