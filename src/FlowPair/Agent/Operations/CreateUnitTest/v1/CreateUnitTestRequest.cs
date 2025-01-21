using System.IO.Abstractions;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest.v1;

public sealed record CreateUnitTestRequest(
    IFileInfo FileInfo,
    IFileInfo? ExampleFileInfo,
    IDirectoryInfo RootPath);
