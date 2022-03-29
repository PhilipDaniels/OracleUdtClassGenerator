using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

public class GeneratedFile
{
    /// <summary>
    /// Name of the generated file under the Analyzers node, e.g. "Person.g.cs"
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The C# source code.
    /// </summary>
    public string SourceCode { get; set; }

    public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();
}
