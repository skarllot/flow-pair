﻿using Ciandt.FlowTools.FlowPair.Agent.Operations.CreateUnitTest;
using Ciandt.FlowTools.FlowPair.Agent.Operations.Login;
using Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;
using Ciandt.FlowTools.FlowPair.Agent.Operations.UpdateUnitTest;
using Ciandt.FlowTools.FlowPair.DependencyInjection;
using Ciandt.FlowTools.FlowPair.Settings.Operations.Configure;
using ConsoleAppFramework;

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
