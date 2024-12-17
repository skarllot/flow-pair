using System.IO.Abstractions;
using System.Text.Json;
using AutomaticInterface;
using Ciandt.FlowTools.FlowReviewer.Common;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models;
using Ciandt.FlowTools.FlowReviewer.Persistence.Models.v1;
using Spectre.Console;

namespace Ciandt.FlowTools.FlowReviewer.Persistence;

public partial interface IUserSessionService;

[GenerateAutomaticInterface]
public sealed class UserSessionService(
    IAnsiConsole console,
    IFileSystem fileSystem,
    AppJsonContext jsonContext)
    : IUserSessionService
{
    private static readonly Version s_latestVersion = new(1, 0);
    private const string SessionFileName = "session.json";

    private readonly IFileInfo _sessionFile =
        fileSystem.FileInfo.New(fileSystem.Path.Combine(ApplicationData.GetPath(fileSystem), SessionFileName));

    public Result<UserSession, string> UserSession { get; private set; } = "User session file not read";

    public Option<Unit> Load()
    {
        if (!_sessionFile.Exists)
        {
            UserSession = CreateSessionFile();
            return Unit();
        }

        UserSession = ReadSessionFile() ?? CreateSessionFile();
        return Unit();
    }

    public void Save(UserSession session)
    {
        _sessionFile.Directory?.Create();

        using var stream = _sessionFile.Open(FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(stream, session, jsonContext.UserSession);

        UserSession = session;
    }

    private UserSession? ReadSessionFile()
    {
        try
        {
            using var stream = _sessionFile.OpenRead();
            var userSession = JsonSerializer.Deserialize(stream, jsonContext.UserSession);
            if (userSession?.Version > s_latestVersion)
            {
                console.MarkupLine($"[bold]Unknown session file version:[/] {userSession.Version}");
                return null;
            }

            return userSession;
        }
        catch (JsonException exception)
        {
            console.MarkupLine($"[bold]Invalid session file:[/] {exception.Message}");
            return null;
        }
    }

    private UserSession CreateSessionFile()
    {
        _sessionFile.Directory?.Create();

        var session = new UserSession(s_latestVersion, "", DateTimeOffset.MinValue);
        using var stream = _sessionFile.Open(FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(stream, session, jsonContext.UserSession);

        return session;
    }
}
