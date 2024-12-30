using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;
using Jab;

namespace Ciandt.FlowTools.FlowPair.LocalFileSystem.Infrastructure;

[ServiceProviderModule]
[Singleton(typeof(IWorkingDirectoryWalker), typeof(WorkingDirectoryWalker))]
[Singleton(typeof(ITempFileWriter), typeof(TempFileWriter))]
[Singleton(typeof(IGetDirectoryStructureHandler), typeof(GetDirectoryStructureHandler))]
public interface ILocalFileSystemModule;
