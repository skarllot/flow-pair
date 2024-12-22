using System.Text;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable CheckNamespace

namespace Microsoft.CodeAnalysis
{
    public abstract class SourceProductionContext
    {
        public CancellationToken CancellationToken { get; }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            throw new NotSupportedException();
        }

        public void AddSource(string sourceName, SourceText sourceText)
        {
            throw new NotSupportedException();
        }
    }

    public abstract class Diagnostic;

    namespace Text
    {
        public abstract class SourceText
        {
            public static SourceText From(string content, Encoding encoding)
            {
                throw new NotSupportedException();
            }
        }
    }
}
