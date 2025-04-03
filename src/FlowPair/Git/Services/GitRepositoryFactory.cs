using System.IO.Abstractions;
using AutomaticInterface;
using LibGit2Sharp;

namespace Raiqub.LlmTools.FlowPair.Git.Services;

public partial interface IGitRepositoryFactory;

[GenerateAutomaticInterface]
public class GitRepositoryFactory : IGitRepositoryFactory
{
    public IRepository Create(IDirectoryInfo directoryInfo)
    {
        return new Repository(directoryInfo.FullName);
    }
}
