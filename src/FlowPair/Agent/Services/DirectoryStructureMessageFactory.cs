using System.IO.Abstractions;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
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
            Role.User,
            $"""
             The repository has the following directory structure:
             ```
             {getDirectoryStructureHandler.Execute(directory)}
             ```
             """);
    }
}
