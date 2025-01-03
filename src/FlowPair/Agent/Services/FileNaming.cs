using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Agent.Services;

public static class FileNaming
{
    public static ImmutableList<string> ProjectExtensions { get; } =
    [
        ".csproj", ".slnx", // C#
        ".Rproj", // R
        ".xcodeproj", ".xcworkspace", // Swift
        ".project", // Java (Eclipse)
        ".workspace", // C++ (CodeBlocks)
        ".idea", // Kotlin, Scala (IntelliJ IDEA)
        ".prj", // MATLAB
    ];

    public static ImmutableList<string> ProjectFiles { get; } =
    [
        "Directory.Packages.props", "Directory.Build.props", "Directory.Build.targets", // C#
        "pom.xml", "build.gradle", // Java (Maven, Gradle)
        "pyproject.toml", "setup.py", // Python
        "package.json", // JavaScript
        "CMakeLists.txt", "Makefile", // C++
        "composer.json", // PHP
        "Gemfile", // Ruby
        "Package.swift", // Swift
        "DESCRIPTION", // R
        "build.gradle.kts", // Kotlin
        "tsconfig.json", // TypeScript
        "go.mod", // Go (Golang)
        "Cargo.toml", // Rust
        "build.sbt", // Scala
        "pubspec.yaml", // Dart
        "Makefile.PL", "dist.ini", // Perl
    ];
}
