namespace Ciandt.FlowTools.FlowPair.Support.Persistence;

public interface IVersionedJsonValue
{
    Version Version { get; }

    static abstract Version CurrentVersion { get; }
}
