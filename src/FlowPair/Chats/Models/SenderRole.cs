using System.Text.Json.Serialization;
using FxKit.CompilerServices;
using Raiqub.Generators.EnumUtilities;

namespace Raiqub.LlmTools.FlowPair.Chats.Models;

public partial class SenderRoleJsonConverter : JsonConverter<SenderRole>;

/// <summary>
/// Represents the role of a sender in a conversation.
/// </summary>
[JsonConverterGenerator(AllowIntegerValues = false)]
[JsonConverter(typeof(SenderRoleJsonConverter))]
[EnumMatch]
public enum SenderRole
{
    /// <summary>
    /// Represents a system message, typically used for setting context or providing instructions.
    /// </summary>
    [JsonPropertyName("system")] System,

    /// <summary>
    /// Represents a message from the user or human interacting with the system.
    /// </summary>
    [JsonPropertyName("user")] User,

    /// <summary>
    /// Represents a message from the AI assistant or model responding to the user.
    /// </summary>
    [JsonPropertyName("assistant")] Assistant,

    /// <summary>
    /// Represents a message related to a function call or API interaction.
    /// </summary>
    [JsonPropertyName("function")] Function
}
