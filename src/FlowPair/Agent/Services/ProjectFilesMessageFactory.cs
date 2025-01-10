using System.IO.Abstractions;
using System.Text;
using AutomaticInterface;
using Raiqub.LlmTools.FlowPair.Chats.Models;
using Raiqub.LlmTools.FlowPair.Common;
using Raiqub.LlmTools.FlowPair.LocalFileSystem.Services;

namespace Raiqub.LlmTools.FlowPair.Agent.Services;

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
            SenderRole.User,
            workingDirectoryWalker
                .FindFilesByExtension(rootDirectory, extensions ?? FileNaming.ProjectExtensions)
                .Concat(workingDirectoryWalker.FindFilesByName(rootDirectory, filenames ?? FileNaming.ProjectFiles))
                .Aggregate(
                    new StringBuilder("The repository has the following project files:").AppendLine(),
                    (curr, next) => AggregateFileContent(curr, next, rootDirectory))
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

        sb.Append("* File: ");
        sb.Append(rootDirectory.GetRelativePath(fileInfo.FullName));
        sb.AppendLine();
        sb.AppendLine("```");
        fileInfo.ReadAllTextTo(sb);
        sb.AppendLine();
        sb.AppendLine("```");
        return sb;
    }
}
