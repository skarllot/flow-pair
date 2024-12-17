using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.ChangeTracking;
using Ciandt.FlowTools.FlowReviewer.Flow;
using Ciandt.FlowTools.FlowReviewer.Persistence;

namespace Ciandt.FlowTools.FlowReviewer;

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
