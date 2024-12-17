using System.Text.Json.Serialization;
using Ciandt.FlowTools.FlowReviewer.Flow;
using Ciandt.FlowTools.FlowReviewer.Persistence;

namespace Ciandt.FlowTools.FlowReviewer.Common;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(AppConfiguration))]
[JsonSerializable(typeof(UserSession))]
[JsonSerializable(typeof(GenerateTokenRequest))]
[JsonSerializable(typeof(GenerateTokenResponse))]
public partial class AppJsonContext : JsonSerializerContext;
