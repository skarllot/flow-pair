using Ciandt.FlowTools.FlowReviewer;
using Ciandt.FlowTools.FlowReviewer.DependencyInjection;

using var container = new AppContainer();
container.GetService<IRunner>().Run();
