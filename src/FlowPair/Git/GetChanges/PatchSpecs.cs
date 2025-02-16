using LibGit2Sharp;

namespace Raiqub.LlmTools.FlowPair.Git.GetChanges;

public static class PatchSpecs
{
    public static Func<PatchEntryChanges, bool> IsChangedCode { get; } = p =>
        !p.IsBinaryComparison && p.Status != ChangeKind.Deleted && p.Mode != Mode.Directory;
}
