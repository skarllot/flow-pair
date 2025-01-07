using ConsoleAppFramework;
using Raiqub.LlmTools.FlowPair.Agent.Operations.CreateUnitTest;
using Raiqub.LlmTools.FlowPair.Agent.Operations.Login;
using Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges;
using Raiqub.LlmTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Raiqub.LlmTools.FlowPair.DependencyInjection;
using Raiqub.LlmTools.FlowPair.Settings.Operations.Configure;

using var container = new AppContainer();
ConsoleApp.ServiceProvider = container;
ConsoleApp.Version = ThisAssembly.AssemblyInformationalVersion;

var app = ConsoleApp.Create();
app.Add<ConfigureCommand>();
app.Add<LoginCommand>();
app.Add<ReviewChangesCommand>();
app.Add<CreateUnitTestCommand>();
app.Add<UpdateUnitTestCommand>();
app.Run(args);
