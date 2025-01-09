using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.Flow.Contracts;
using Raiqub.LlmTools.FlowPair.Flow.Infrastructure;
using Raiqub.LlmTools.FlowPair.Flow.Operations.GenerateToken;
using Raiqub.LlmTools.FlowPair.Settings.Contracts.v1;
using Raiqub.LlmTools.FlowPair.Settings.Services;
using Raiqub.LlmTools.FlowPair.Support.Persistence;
using Raiqub.LlmTools.FlowPair.UserSessions.Contracts.v1;
using Raiqub.LlmTools.FlowPair.UserSessions.Services;
using Spectre.Console;

namespace Raiqub.LlmTools.FlowPair.Agent.Operations.Login;

public partial interface ILoginUseCase;

[GenerateAutomaticInterface]
public sealed class LoginUseCase(
    TimeProvider timeProvider,
    IAnsiConsole console,
    FlowHttpClient httpClient,
    IAppSettingsRepository appSettingsRepository,
    IUserSessionRepository userSessionRepository,
    IFlowGenerateTokenHandler generateTokenHandler)
    : ILoginUseCase
{
    public Result<UserSession, int> Execute(bool isBackground)
    {
        var result = from config in appSettingsRepository.Read().MapErr(HandleConfigurationError)
            from session in userSessionRepository.Read()
                .Do(SetupHttpAuthentication)
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

    private void SetupHttpAuthentication(UserSession session)
    {
        if (session.IsExpired(timeProvider))
            return;

        httpClient.BearerToken = session.AccessToken;
    }

    private Result<UserSession, FlowError> RequestToken(
        AppConfiguration configuration,
        UserSession userSession,
        bool verbose)
    {
        if (!verbose)
        {
            return generateTokenHandler.Execute(configuration, userSession);
        }

        return console.Status()
            .Start(
                "Generating access token",
                _ => generateTokenHandler.Execute(configuration, userSession))
            .Do(_ => console.WriteLine("New access token generated"));
    }

    private int HandleFlowError(FlowError error)
    {
        console.MarkupLineInterpolated($"[red]Error:[/] {error.FullMessage}");
        return 2;
    }
}
