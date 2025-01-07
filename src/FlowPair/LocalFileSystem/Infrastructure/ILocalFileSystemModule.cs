using Jab;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.LocalFileSystem.Infrastructure;

[ServiceProviderModule]
[Singleton(typeof(IWorkingDirectoryWalker), typeof(WorkingDirectoryWalker))]
[Singleton(typeof(ITempFileWriter), typeof(TempFileWriter))]
[Singleton(typeof(IGetDirectoryStructureHandler), typeof(GetDirectoryStructureHandler))]
public interface ILocalFileSystemModule;
