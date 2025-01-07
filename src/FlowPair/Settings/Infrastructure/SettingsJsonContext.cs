using System.Text.Json.Serialization;
using Raiqub.LlmTools.FlowPair.Settings.Contracts.v1;

namespace Raiqub.LlmTools.FlowPair.Settings.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
public partial class SettingsJsonContext : JsonSerializerContext;
