using Ciandt.FlowTools.FlowPair;
using Ciandt.FlowTools.FlowPair.DependencyInjection;

using var container = new AppContainer();
container.GetService<IRunner>().Run();
