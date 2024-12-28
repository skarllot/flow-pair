using System.Collections.Immutable;
using FxKit.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Agent.Models;

[Union]
public partial record Instruction
{
    partial record StepInstruction(string Messsage);

    partial record MultiStepInstruction(string Preamble, ImmutableList<string> Messages, string Ending);

    partial record JsonConvertInstruction(string Message);
}
