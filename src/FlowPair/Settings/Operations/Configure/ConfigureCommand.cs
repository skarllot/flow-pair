using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Settings.Contracts.v1;
using Ciandt.FlowTools.FlowPair.Settings.Services;
using ConsoleAppFramework;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Settings.Operations.Configure;

public class ConfigureCommand(
    IAnsiConsole console,
    IAppSettingsRepository appSettingsRepository)
{
    /// <summary>
    /// Configure Flow authentication.
    /// </summary>
    /// <param name="tenant">-t, Tenant name.</param>
    /// <param name="clientId">-u, Client ID.</param>
    /// <param name="clientSecret">-p, Client secret.</param>
    [Command("configure")]
    public void Execute(
        string? tenant = null,
        string? clientId = null,
        string? clientSecret = null)
    {
        var originalSettings = appSettingsRepository.Read().UnwrapOrNull();

        tenant ??= originalSettings?.Tenant ??
                   console.Prompt(new TextPrompt<string>("Tenant:") { AllowEmpty = false });
        clientId ??= originalSettings?.ClientId ??
                     console.Prompt(new TextPrompt<string>("Client ID:") { AllowEmpty = false });
        clientSecret ??= originalSettings?.ClientSecret ??
                         console.Prompt(new TextPrompt<string>("Client Secret:") { AllowEmpty = false });

        appSettingsRepository.Save(
            originalSettings is null
                ? AppConfiguration.New(
                    tenant: tenant,
                    clientId: clientId,
                    clientSecret: clientSecret)
                : originalSettings with
                {
                    Version = AppConfiguration.CurrentVersion,
                    Tenant = tenant,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                });
    }
}
