using System.IO.Abstractions;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Settings.Contracts.v1;
using Ciandt.FlowTools.FlowPair.Settings.Infrastructure;
using Ciandt.FlowTools.FlowPair.Support.Persistence;

namespace Ciandt.FlowTools.FlowPair.Settings.Services;

public partial interface IAppSettingsRepository;

[GenerateAutomaticInterface]
public sealed class AppSettingsRepository(
    IFileSystem fileSystem,
    SettingsJsonContext jsonContext)
    : JsonFileRepository<AppConfiguration>(fileSystem, jsonContext.AppConfiguration, ConfigFileName),
        IAppSettingsRepository
{
    private const string ConfigFileName = "appsettings.json";
}
