using System.IO.Abstractions;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Support.Persistence;
using Raiqub.LlmTools.FlowPair.UserSessions.Contracts.v1;
using Raiqub.LlmTools.FlowPair.UserSessions.Infrastructure;

namespace Raiqub.LlmTools.FlowPair.UserSessions.Services;

public partial interface IUserSessionRepository;

[GenerateAutomaticInterface]
public sealed class UserSessionRepository(
    IFileSystem fileSystem,
    UserSessionJsonContext jsonContext)
    : JsonFileRepository<UserSession>(fileSystem, jsonContext.UserSession, SessionFileName),
        IUserSessionRepository
{
    private const string SessionFileName = "session.json";
}
