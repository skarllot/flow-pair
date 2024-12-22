using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Ciandt.FlowTools.FlowPair.Common;

namespace Ciandt.FlowTools.FlowPair.Support.Persistence;

public abstract class JsonFileRepository<TValue>(
    IFileSystem fileSystem,
    JsonTypeInfo<TValue> jsonTypeInfo,
    string filename)
    where TValue : class, IVersionedJsonValue
{
    private TValue? _valueCache;

    public Result<TValue, GetJsonFileValueError> Read()
    {
        if (_valueCache is not null)
        {
            return _valueCache;
        }

        var appData = ApplicationData.GetAppDataDirectory(fileSystem);
        appData.Create();

        return from file in appData.NewFileIfExists(filename)
                .OkOrElse(GetJsonFileValueError.NotFound.λ)
            from config in ReadConfigurationFile(file)
                .Do(c => _valueCache = c)
            select config;
    }

    public Unit Save(TValue value)
    {
        var appData = ApplicationData.GetAppDataDirectory(fileSystem);
        appData.Create();

        var file = appData.NewFile(filename);
        using var stream = file.Open(FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(stream, value, jsonTypeInfo);

        _valueCache = value;
        return Unit();
    }

    private Result<TValue, GetJsonFileValueError> ReadConfigurationFile(IFileInfo configurationFile)
    {
        try
        {
            using var stream = configurationFile.OpenRead();
            var configuration = JsonSerializer.Deserialize(stream, jsonTypeInfo);
            if (configuration is null)
            {
                return GetJsonFileValueError.Null.Of();
            }

            if (configuration.Version > TValue.CurrentVersion)
            {
                return GetJsonFileValueError.UnknownVersion.Of(configuration.Version);
            }

            return configuration;
        }
        catch (JsonException exception)
        {
            return GetJsonFileValueError.Invalid.Of(exception);
        }
    }
}
