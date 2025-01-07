using System.Text.Json;
using FxKit.CompilerServices;

namespace Raiqub.LlmTools.FlowPair.UserSessions.Services;

[Union]
public partial record GetUserSessionError
{
    partial record NotFound;
    partial record Invalid(JsonException Exception);

    partial record Null;
    partial record UnknownVersion(Version Version);
}
