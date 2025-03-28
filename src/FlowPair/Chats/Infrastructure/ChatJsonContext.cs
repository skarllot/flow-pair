using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Raiqub.LlmTools.FlowPair.Chats.Models;

namespace Raiqub.LlmTools.FlowPair.Chats.Infrastructure;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(ImmutableList<ImmutableList<Message>>))]
public partial class ChatJsonContext : JsonSerializerContext;
