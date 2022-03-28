using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor FoundFile = new DiagnosticDescriptor(
    id: "ORAUDT01",
    title: "Found .oraudt file",
    messageFormat: "Found file {0}.oraudt file",
    category: "OracleUdtClassGenerator",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor EmptyFile = new DiagnosticDescriptor(
        id: "ORAUDT02",
        title: ".oraudt file is empty",
        messageFormat: "The {0}.oraudt file is empty",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
        );

    public static Diagnostic MakeFoundFileDiagnostic(string filename)
    {
        return Diagnostic.Create(Diagnostics.FoundFile, Location.None, filename);
    }

    public static Diagnostic MakeEmptyFileDiagnostic(string filename)
    {
        return Diagnostic.Create(Diagnostics.EmptyFile, Location.None, filename);
    }
}
