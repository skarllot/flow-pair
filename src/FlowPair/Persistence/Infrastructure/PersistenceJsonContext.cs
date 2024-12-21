using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure.v1;

namespace Ciandt.FlowTools.FlowPair.Persistence.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
public partial class PersistenceJsonContext : JsonSerializerContext;
