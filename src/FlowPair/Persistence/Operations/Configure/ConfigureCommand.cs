using Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure.v1;
using Ciandt.FlowTools.FlowPair.Persistence.Services;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Persistence.Operations.Configure;

public class ConfigureCommand(
    IAnsiConsole console,
    IAppSettingsRepository appSettingsRepository)
{
    /// <summary>
    /// Configure Flow authentication.
    /// </summary>
    /// <param name="tenant">-t, Tenant name.</param>
    /// <param name="clientId">-i, Client ID.</param>
    /// <param name="clientSecret">-s, Client secret.</param>
    [Command("configure")]
    public void Execute(
        string? tenant = null,
        string? clientId = null,
        string? clientSecret = null)
    {
        tenant ??= console.Prompt(new TextPrompt<string>("Tenant:") { AllowEmpty = false });
        clientId ??= console.Prompt(new TextPrompt<string>("Client ID:") { AllowEmpty = false });
        clientSecret ??= console.Prompt(new TextPrompt<string>("Client Secret:") { AllowEmpty = false });

        appSettingsRepository.SaveConfiguration(
            AppConfiguration.New(
                tenant: tenant,
                clientId: clientId,
                clientSecret: clientSecret));
    }
}
