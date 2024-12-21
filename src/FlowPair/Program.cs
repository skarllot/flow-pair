using Ciandt.FlowTools.FlowPair.DependencyInjection;
using Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure;
using ConsoleAppFramework;

using var container = new AppContainer();
ConsoleApp.ServiceProvider = container;
ConsoleApp.Version = ThisAssembly.AssemblyInformationalVersion;

var app = ConsoleApp.Create();
app.Add<ConfigureCommand>();
app.Run(args);
