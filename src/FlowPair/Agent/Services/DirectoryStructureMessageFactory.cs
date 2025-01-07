using System.IO.Abstractions;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.GetDirectoryStructure;

namespace Raiqub.LlmTools.FlowPair.Agent.Services;

public partial interface IDirectoryStructureMessageFactory;

[GenerateAutomaticInterface]
public sealed class DirectoryStructureMessageFactory(
    IGetDirectoryStructureHandler getDirectoryStructureHandler)
    : IDirectoryStructureMessageFactory
{
    public Message CreateWithRepositoryStructure(IDirectoryInfo directory)
    {
        return new Message(
            SenderRole.User,
            $"""
             The repository has the following directory structure:
             ```
             {getDirectoryStructureHandler.Execute(directory)}
             ```
             """);
    }
}
