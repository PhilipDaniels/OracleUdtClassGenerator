using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

/// <summary>
/// Represents the results of processing a single .oraudt file.
/// </summary>
public class ProcessedOraUdtFile
{
    public List<GeneratedFile> GeneratedFiles { get; } = new List<GeneratedFile>();
    public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();
}
