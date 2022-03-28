using System;
using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor FoundFile = new DiagnosticDescriptor(
        id: "ORAUDT01",
        title: "Found .oraudt file",
        messageFormat: "Found file {0}",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
        );

    public static readonly DiagnosticDescriptor FoundSpec = new DiagnosticDescriptor(
        id: "ORAUDT02",
        title: "Found spec",
        messageFormat: "Found spec for {0} with {1} fields",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
        );

    public static readonly DiagnosticDescriptor EmptyFile = new DiagnosticDescriptor(
        id: "ORAUDT03",
        title: ".oraudt file is empty",
        messageFormat: "The file {0} is empty",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
        );

    public static readonly DiagnosticDescriptor ExceptionOccurred = new DiagnosticDescriptor(
        id: "ORAUDT04",
        title: "Exception while processing oraudt",
        messageFormat: "An exception occurred: {0}",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
        );

    public static readonly DiagnosticDescriptor CouldNotDetermineNamespace = new DiagnosticDescriptor(
        id: "ORAUDT05",
        title: "Could not determine namespace",
        messageFormat: "Could not determine namespace from project folder structure, using default of {0}",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
        );

    public static readonly DiagnosticDescriptor GeneratedFile = new DiagnosticDescriptor(
        id: "ORAUDT06",
        title: "Generated File",
        messageFormat: "Generated file {0} in namespace {1}",
        category: "OracleUdtClassGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
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

    public static Diagnostic MakeFoundSpecDiagnostic(string className, int fieldCount)
    {
        return Diagnostic.Create(Diagnostics.FoundSpec, Location.None, className, fieldCount);
    }

    public static Diagnostic MakeExceptionOccurredDiagnostic(Exception ex)
    {
        return Diagnostic.Create(Diagnostics.ExceptionOccurred, Location.None, ex.ToString());
    }

    public static Diagnostic MakeCouldNotDetermineNamespaceDiagnostic(string namespaceName)
    {
        return Diagnostic.Create(Diagnostics.CouldNotDetermineNamespace, Location.None, namespaceName);
    }

    public static Diagnostic MakeGeneratedFileDiagnostic(string filename, string namespaceName)
    {
        return Diagnostic.Create(Diagnostics.GeneratedFile, Location.None, filename, namespaceName);
    }
}
