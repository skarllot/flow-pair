using System.IO.Abstractions;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Chats.Models;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.GetDirectoryStructure;

namespace Ciandt.FlowTools.FlowPair.Agent.Services;

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
