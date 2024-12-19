using System.Collections.Immutable;

namespace Ciandt.FlowTools.FlowReviewer.Agent.ReviewChanges;

public sealed record Instructions(
    string Name,
    ImmutableArray<string> Extensions,
    string Message)
{
    public static readonly ImmutableList<Instructions> Default =
    [
        new Instructions(
            "Generic programming instructions",
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
                /* Shell Scripting */".sh", ".bash", ".zsh", ".ksh", ".csh", ".tcsh", ".fish"
            ],
            """
            You are an expert developer, your task is to review a set of pull requests on Azure DevOps.

            You are given a set of Git patches, containing the filenames and their partial contents. Note that you might not have the full context of the code.

            Only review lines of code which have been changed (added or removed) in the pull request. Lines which have been removed have the type `REMOVED` and lines which have been added have the type `ADDED`. Other lines are added to provide context but should be ignored in the review.

            Begin your feedback by evaluating the changed code using a risk score similar to a LOGAF score but measured from 0 to 3, where 0 is the lowest risk to the codebase if the code is merged and 3 is the highest risk which would likely break something or be unsafe. Risk score should be described as "0 - Not important", "1 - Low priority adjustments", "2 - Medium priority adjustments" or "3 - High priority adjustments".

            In your feedback:
            1. Focus exclusively on highlighting potential bugs.
            2. Improve readability only if it significantly impacts understanding.
            3. Make code cleaner only if it introduces substantial benefits.
            4. Maximize the performance of the code if there is a clear, impactful improvement.
            5. Flag any API keys or secrets present in plain text immediately as highest risk.
            6. Rate the changes based on SOLID principles if applicable.
            7. Apply the principles of DRY, KISS, YAGNI and Clean Code during the review of the code.
            8. Do not infer unknown code, do not speculate the referenced code.
            9. Avoid magic strings and numbers.
            10. New code should follow existing patterns and structure.

            Only provide feedback on critical issues. If the code is already well-written or issues are minor, do not provide any feedback.

            Avoid commenting on breaking functions down into smaller, more manageable functions unless it is a significant problem. Be aware that there will be libraries and techniques used which you might not be familiar with, so do not comment on those unless you are confident that there is a problem.

            Do not provide positive reinforcement or comments on good decisions. Focus solely on areas that need improvement.

            Your feedbacks will be input in Azure DevOps via API `/comments` endpoint. The feedbacks should be in a valid JSON format.

            Use markdown formatting for the feedback details. Do not include the filename or risk level in the feedback details.

            Ensure the feedback details are brief, concise, and accurate. If there are multiple similar issues, only comment on the most critical.

            Include brief example code snippets in the feedback details for your suggested changes when you're confident your suggestions are improvements. Use the same programming language as the file under review. If there are multiple improvements you suggest in the feedback details, use an ordered list to indicate the priority of the changes.

            It is not necessary to add low-risk comments that are not relevant to changes in the pull request.

            The message in the field text must be in English.

            Format the response in a valid JSON format as a list of feedbacks. Remember it is crucial that the result has the file path.
            This valid JSON is going to be inserted in a value of a key-value from another JSON object, be-aware about the formatting. Remember to only list feedbacks that needs user action.
            The schema of the JSON feedback object must be:
            ```json
            [
              {
                    "riskScore": 0,
                    "riskDescription": "Not important",
                    "feedback": "",
                    "path": "/path/path/file.extension",
                    "line": 16,
                    "lineType": "ADDED"
                },
                {
                    "riskScore": 1,
                    "riskDescription": "Low priority adjustments",
                    "feedback": "",
                    "path": "/path/path/file.extension",
                    "line": 20,
                    "lineType": "ADDED"
                }
            ]
            ```
            """)
    ];

    public static Option<Instructions> FindInstructionsForFile(
        IReadOnlyList<Instructions> instructions,
        string filePath)
    {
        return instructions
            .Reverse()
            .FirstOrDefault(i => i.Extensions.Any(s => filePath.EndsWith(s, StringComparison.OrdinalIgnoreCase)));
    }
}
