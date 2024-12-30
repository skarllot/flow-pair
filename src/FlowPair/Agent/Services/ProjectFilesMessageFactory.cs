using System.IO.Abstractions;
using System.Text;
using AutomaticInterface;
using Ciandt.FlowTools.FlowPair.Common;
using Ciandt.FlowTools.FlowPair.Flow.Operations.ProxyCompleteChat.v1;
using Ciandt.FlowTools.FlowPair.LocalFileSystem.Services;

namespace Ciandt.FlowTools.FlowPair.Agent.Services;

public partial interface IProjectFilesMessageFactory;

[GenerateAutomaticInterface]
public sealed class ProjectFilesMessageFactory(
    IWorkingDirectoryWalker workingDirectoryWalker)
    : IProjectFilesMessageFactory
{
    public Message CreateWithProjectFilesContent(
        IDirectoryInfo rootDirectory,
        IEnumerable<string>? extensions = null,
        IEnumerable<string>? filenames = null)
    {
        return new Message(
            Role.User,
            workingDirectoryWalker
                .FindFilesByExtension(rootDirectory, extensions ?? FileNaming.ProjectExtensions)
                .Concat(workingDirectoryWalker.FindFilesByName(rootDirectory, filenames ?? FileNaming.ProjectFiles))
                .Aggregate(new StringBuilder(), (curr, next) => AggregateFileContent(curr, next, rootDirectory))
                .ToString());
    }

    private static StringBuilder AggregateFileContent(
        StringBuilder sb,
        IFileInfo fileInfo,
        IDirectoryInfo rootDirectory)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        sb.Append("File: ");
        sb.Append(rootDirectory.GetRelativePath(fileInfo.FullName));
        sb.AppendLine();
        sb.AppendLine("```");
        fileInfo.ReadAllTextTo(sb);
        sb.AppendLine();
        sb.AppendLine("```");
        return sb;
    }
}
