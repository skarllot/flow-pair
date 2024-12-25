using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowPair.Agent.Operations.ReviewChanges;

public sealed record ChatScript(
    string Name,
    ImmutableArray<string> Extensions,
    string SystemInstruction,
    ImmutableList<Instruction> Instructions)
{
    public const string StopKeywordPlaceholder = "<NO FEEDBACK>";
    public static readonly ImmutableList<ChatScript> Default =
    [
        new(
            "Generic programming chat script",
            [
                /* Python          */".py", ".pyw", ".pyx", ".pxd", ".pxi",
                /* JavaScript      */".js", ".jsx", ".mjs", ".cjs",
                /* Java            */".java",
                /* C#              */".cs", ".csx",
                /* C++             */".cpp", ".cxx", ".cc", ".c++", ".hpp", ".hxx", ".h", ".hh", ".h++",
                /* PHP             */".php", ".phtml", ".phps",
                /* Ruby            */".rb", ".rbw", ".rake",
                /* Swift           */".swift",
                /* R               */".r",
                /* SQL             */".sql",
                /* Kotlin          */".kt", ".kts",
                /* TypeScript      */".ts", ".tsx",
                /* Go (Golang)     */".go",
                /* Rust            */".rs",
                /* Scala           */".scala", ".sc",
                /* Dart            */".dart",
                /* Perl            */".pl", ".pm", ".t", ".pod",
                /* MATLAB          */".m",
                /* VBA             */".bas", ".cls", ".frm",
                /* Shell Scripting */".sh", ".bash", ".zsh", ".ksh", ".csh", ".tcsh", ".fish",
            ],
            """
            You are an expert developer, your task is to review a set of changes on Git commits.
            You are given a set of Git patches, containing the filenames and their partial contents. Note that you might not have the full context of the code.
            Only review lines of code which have been changed (added or removed) in the pull request. Other lines are added to provide context but should be ignored in the review.
            Begin your feedback by evaluating the changed code using a risk score similar to a LOGAF score but measured from 0 to 3, where 0 is the lowest risk to the codebase if the code is merged and 3 is the highest risk which would likely break something or be unsafe. Risk score should be described as "0 - Not important", "1 - Low priority adjustments", "2 - Medium priority adjustments" or "3 - High priority adjustments".
            Only provide feedback on critical issues. If the code is already well-written or issues are minor, do not provide any feedback.
            Avoid commenting on breaking functions down into smaller, more manageable functions unless it is a significant problem. Be aware that there will be libraries and techniques used which you might not be familiar with, so do not comment on those unless you are confident that there is a problem.
            """,
            [
                Instruction.MultiStepInstruction.Of(
                    "Give feedback to ",
                    [
                        "improve readability where it can significantly impacts understanding",
                        "make code cleaner where it introduces substantial benefits",
                        "maximize the performance of the code where there is a clear, impactful improvement",
                        "flag any API keys or secrets present in plain text immediately as highest risk",
                        "rate the changes based on SOLID principles",
                        "apply the principles of DRY, KISS, YAGNI and Clean Code",
                        "avoid magic strings and numbers",
                        "ensure new code follow existing patterns and structure",
                    ],
                    $" if applicable; otherwise, reply with \"{StopKeywordPlaceholder}\" when there are no suggestions"),
                Instruction.StepInstruction.Of(
                    """
                    Ensure the feedback contain the file path and the line number.
                    Do not provide positive reinforcement or comments on good decisions. Focus solely on areas that need improvement.
                    """),
                Instruction.StepInstruction.Of(
                    """
                    Ensure the feedback details are brief, concise, and accurate. If there are multiple similar issues, only comment on the most critical.
                    Include brief example code snippets in the feedback details for your suggested changes when you're confident your suggestions are improvements.
                    Use the same programming language as the file under review. If there are multiple improvements you suggest in the feedback details, use an ordered list to indicate the priority of the changes.
                    """),
                Instruction.StepInstruction.Of(
                    """
                    Ensure the message in the feedback is in English.
                    Ensure the feedback do not infer unknown code, do not speculate the referenced code.
                    """),
                Instruction.JsonConvertInstruction.Of(
                    """
                    Format the feedback in a valid JSON format as a list of feedbacks, or "[]" for no feedbacks.
                    The "feedback" property can be multiline and include example code snippets.
                    The schema of the JSON feedback object must be:
                    """),
            ]),
    ];

    public static Option<ChatScript> FindChatScriptForFile(
        IReadOnlyList<ChatScript> scripts,
        string filePath)
    {
        return scripts
            .Reverse()
            .FirstOrDefault(i => i.Extensions.Any(s => filePath.EndsWith(s, StringComparison.OrdinalIgnoreCase)));
    }
}
