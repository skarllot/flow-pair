using System.IO.Abstractions;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Support.Persistence;
using Ciandt.FlowTools.FlowPair.UserSessions.Contracts.v1;
using Ciandt.FlowTools.FlowPair.UserSessions.Infrastructure;

namespace Ciandt.FlowTools.FlowPair.UserSessions.Services;

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
