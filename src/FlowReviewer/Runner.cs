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
    ILlmClient llmClient)
    : IRunner
{
    public void Run()
    {
        _ = from config in configurationService.ReadOrCreate()
            from session in userSessionService.Load()
            from diff in gitDiffExtractor.Extract()
            from llm in llmClient.Prompt(diff)
            select Unit();
    }
}
