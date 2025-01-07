using System.Text.Json.Serialization;
using Raiqub.LlmTools.FlowPair.UserSessions.Contracts.v1;

namespace Raiqub.LlmTools.FlowPair.UserSessions.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(UserSession))]
public partial class UserSessionJsonContext : JsonSerializerContext;
