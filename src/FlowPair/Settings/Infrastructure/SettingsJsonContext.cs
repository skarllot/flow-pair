using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Settings.Contracts.v1;

namespace Ciandt.FlowTools.FlowPair.Settings.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
public partial class SettingsJsonContext : JsonSerializerContext;
