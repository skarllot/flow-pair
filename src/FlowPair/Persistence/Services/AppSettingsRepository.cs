using System.IO.Abstractions;
using System.Text.Json;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Persistence.Infrastructure;
using Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure.v1;

namespace Ciandt.FlowTools.FlowPair.Persistence.Services;

public partial interface IAppSettingsRepository;

[GenerateAutomaticInterface]
public sealed class AppSettingsRepository(
    IFileSystem fileSystem,
    PersistenceJsonContext jsonContext)
    : IAppSettingsRepository
{
    private const string ConfigFileName = "appsettings.json";
    private AppConfiguration? _configurationCache;

    public Result<AppConfiguration, GetConfigurationError> GetConfiguration()
    {
        if (_configurationCache is not null)
        {
            return _configurationCache;
        }

        var appData = ApplicationData.GetAppDataDirectory(fileSystem);
        appData.Create();

        return from file in appData
                .EnumerateFiles(ConfigFileName, SearchOption.TopDirectoryOnly)
                .FirstOrNone()
                .OkOrElse(GetConfigurationError.NotFound.λ)
            from config in ReadConfigurationFile(file)
                .Do(c => _configurationCache = c)
            select config;
    }

    public void SaveConfiguration(AppConfiguration configuration)
    {
        var appData = ApplicationData.GetAppDataDirectory(fileSystem);
        appData.Create();

        var file = appData.CreateFile(ConfigFileName);
        using var stream = file.Open(FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(stream, configuration, jsonContext.AppConfiguration);
    }

    private Result<AppConfiguration, GetConfigurationError> ReadConfigurationFile(IFileInfo configurationFile)
    {
        try
        {
            using var stream = configurationFile.OpenRead();
            var configuration = JsonSerializer.Deserialize(stream, jsonContext.AppConfiguration);
            if (configuration is null)
            {
                return GetConfigurationError.Null.Of();
            }

            if (configuration.Version > AppConfiguration.CurrentVersion)
            {
                return GetConfigurationError.UnknownVersion.Of(configuration.Version);
            }

            return configuration;
        }
        catch (JsonException exception)
        {
            return GetConfigurationError.Invalid.Of(exception);
        }
    }
}
