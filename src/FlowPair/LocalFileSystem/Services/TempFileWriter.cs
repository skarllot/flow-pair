using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Support.Persistence;

namespace Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

public partial interface ITempFileWriter;

[GenerateAutomaticInterface]
public sealed class TempFileWriter(
    IFileSystem fileSystem)
    : ITempFileWriter
{
    public IFileInfo Write(string filename, string content)
    {
        var tempPath = ApplicationData.GetTempPath(fileSystem);
        tempPath.Create();

        var newFile = tempPath.NewFile(filename);
        newFile.WriteAllText(content, Encoding.UTF8);

        return newFile;
    }

    public IFileInfo WriteJson<T>(string filename, T value, JsonTypeInfo<T> jsonTypeInfo)
    {
        var tempPath = ApplicationData.GetTempPath(fileSystem);
        tempPath.Create();

        var newFile = tempPath.NewFile(filename);

        using var stream = newFile.Open(FileMode.Create);
        JsonSerializer.Serialize(stream, value, jsonTypeInfo);

        return newFile;
    }
}
