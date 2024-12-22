using System.Text.Json;
using FxKit.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Support.Persistence;

[Union]
public partial record GetJsonFileValueError
{
    partial record NotFound;
    partial record Invalid(JsonException Exception);

    partial record Null;
    partial record UnknownVersion(Version Version);
}
