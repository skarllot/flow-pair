using System.Text.Json;
using FxKit.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Persistence.Services;

[Union]
public partial record GetConfigurationError
{
    partial record NotFound;
    partial record Invalid(JsonException Exception);

    partial record Null;
    partial record UnknownVersion(Version Version);
}
