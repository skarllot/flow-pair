using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Agent.ReviewChanges;
using Ciandt.FlowTools.FlowPair.ChangeTracking;
using Ciandt.FlowTools.FlowPair.Persistence;

namespace Ciandt.FlowTools.FlowPair;

public partial interface IRunner;

[GenerateAutomaticInterface]
public class Runner(
    IConfigurationService configurationService,
    IUserSessionService userSessionService,
    IGitDiffExtractor gitDiffExtractor,
    IFlowChangesReviewer changesReviewer)
    : IRunner
{
    public void Run()
    {
        _ = from config in configurationService.ReadOrCreate()
            from session in userSessionService.Load()
            from diff in gitDiffExtractor.Extract()
            from changes in changesReviewer.Run(diff)
            select Unit();
    }
}
