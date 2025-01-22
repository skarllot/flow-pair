using System.IO.Abstractions;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest.v1;

public sealed record UpdateUnitTestRequest(
    IFileInfo SourceFileInfo,
    IFileInfo TestFileInfo,
    IDirectoryInfo RootPath);
