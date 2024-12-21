using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow;
using Ciandt.FlowTools.FlowPair.Flow.GenerateToken;
using Ciandt.FlowTools.FlowPair.Settings.Contracts.v1;
using Ciandt.FlowTools.FlowPair.Settings.Services;
using Ciandt.FlowTools.FlowPair.Support.Persistence;
using Ciandt.FlowTools.FlowPair.UserSessions.Contracts.v1;
using Ciandt.FlowTools.FlowPair.UserSessions.Services;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.Login;

public partial interface ILoginUseCase;

[GenerateAutomaticInterface]
public sealed class LoginUseCase(
    TimeProvider timeProvider,
    IAnsiConsole console,
    IAppSettingsRepository appSettingsRepository,
    IUserSessionRepository userSessionRepository,
    IFlowAuthService flowAuthService)
    : ILoginUseCase
{
    public Result<UserSession, int> Execute(bool isBackground)
    {
        var result = from config in appSettingsRepository.Read().MapErr(HandleConfigurationError)
            from session in userSessionRepository.Read()
                .UnwrapOrElse(UserSession.Empty)
                .Ensure(s => s.IsExpired(timeProvider), 0)
            from auth in RequestToken(config, session, verbose: isBackground).MapErr(HandleFlowError)
                .Do(s => userSessionRepository.Save(s))
            select auth;

        if (!isBackground)
        {
            result.Do(s => console.WriteLine($"Signed in with expiration at {s.ExpiresAt:g}"));
        }

        return result;
    }

    private int HandleConfigurationError(GetJsonFileValueError error)
    {
        var errorMessage = error.Match<FormattableString>(
            NotFound: _ => $"[red]Error:[/] Configuration not found.",
            Invalid: x => $"[red]Error:[/] {x.Exception.Message}",
            Null: _ => $"[red]Error:[/] Configuration is null.",
            UnknownVersion: x => $"[red]Error:[/] The configuration version '{x.Version}' is not supported.");

        console.MarkupLineInterpolated(errorMessage);
        return 1;
    }

    private int HandleFlowError(FlowError error)
    {
        console.MarkupLineInterpolated($"[red]Error:[/] {error.FullMessage}");
        return 2;
    }

    private Result<UserSession, FlowError> RequestToken(
        AppConfiguration configuration,
        UserSession userSession,
        bool verbose)
    {
        if (verbose)
        {
            console.Write("Signing in to Flow...");
        }

        var result = flowAuthService.RequestToken(configuration, userSession);
        if (verbose)
        {
            result
                .Do(_ => console.WriteLine(" OK"))
                .DoErr(_ => console.WriteLine(" FAIL"));
        }

        return result;
    }
}
