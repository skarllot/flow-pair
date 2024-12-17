using Ciandt.FlowTools.FlowReviewer.ChangeTracking;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Flow;
using Ciandt.FlowTools.FlowReviewer.Flow.AmazonBedrock;
using Ciandt.FlowTools.FlowReviewer.Flow.OpenAi;
using Ciandt.FlowTools.FlowReviewer.Persistence;
using Jab;

namespace Ciandt.FlowTools.FlowReviewer.DependencyInjection;

[ServiceProvider]
[Import(typeof(IExternalModule))]
[Singleton(typeof(AppJsonContext), Factory = nameof(AppJsonContextDefaultInstance))]
[Singleton(typeof(IRunner), typeof(Runner))]
[Singleton(typeof(IConfigurationService), typeof(ConfigurationService))]
[Singleton(typeof(IUserSessionService), typeof(UserSessionService))]
[Singleton(typeof(IGitDiffExtractor), typeof(GitDiffExtractor))]
[Singleton(typeof(IFlowChangesReviewer), typeof(FlowChangesReviewer))]
[Singleton(typeof(ILlmClient), typeof(LlmClient))]
[Singleton(typeof(IOpenAiClient), typeof(OpenAiClient))]
[Singleton(typeof(IBedrockClient), typeof(BedrockClient))]
public sealed partial class AppContainer
{
    private static AppJsonContext AppJsonContextDefaultInstance() => AppJsonContext.Default;
}
