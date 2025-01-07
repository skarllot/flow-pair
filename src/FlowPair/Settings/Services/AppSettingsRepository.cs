using System.IO.Abstractions;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Settings.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Settings.Infrastructure;
using Raiqub.LlmTools.FlowPair.Support.Persistence;

namespace Raiqub.LlmTools.FlowPair.Settings.Services;

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
