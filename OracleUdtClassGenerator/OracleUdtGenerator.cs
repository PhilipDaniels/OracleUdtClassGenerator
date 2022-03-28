using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OracleUdtClassGenerator;

[Generator(LanguageNames.CSharp)]
public class OracleUdtGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // nb. See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md#caching

        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
        var texts = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".oraudt", StringComparison.OrdinalIgnoreCase));
        var textsAndAssembly = texts.Combine(assemblyName);

        var combined = textsAndAssembly.Select(static (file, cancellationToken) =>
        {
            var sourceContents = file.Left.GetText(cancellationToken)!.ToString();
            return new FileAndContents
            {
                AssemblyName = file.Right,
                AdditionalText = file.Left,
                SourceContents = sourceContents,
            };
        });

        var diagnostics = new List<Diagnostic>();

        var generatedFiles = combined.SelectMany((input, token) =>
        {
            return ProcessOraUdtFile(diagnostics, input.AssemblyName, input.AdditionalText, input.SourceContents);
        });

        context.RegisterSourceOutput(generatedFiles, static (spc, generatedFile) =>
        {
            var sourceText = SourceText.From(generatedFile.Contents, Encoding.UTF8);
            spc.AddSource(generatedFile.FileName, sourceText);
        });

        context.RegisterSourceOutput(assemblyName, (spc, _) =>
        {
            foreach (var diag in diagnostics)
            {
                spc.ReportDiagnostic(diag);
            }
        });
    }

    private static IEnumerable<GeneratedFile> ProcessOraUdtFile(List<Diagnostic> diagnostics, string assemblyName, AdditionalText additional, string contents)
    {
        var results = new List<GeneratedFile>();

        try
        {
            diagnostics.Add(Diagnostics.MakeFoundFileDiagnostic(additional.Path));

            if (string.IsNullOrWhiteSpace(contents))
            {
                diagnostics.Add(Diagnostics.MakeEmptyFileDiagnostic(additional.Path));
            }
            else
            {
                var targetSpecs = Grammar.ParseTargetSpecs(contents);
                foreach (var spec in targetSpecs)
                {
                    diagnostics.Add(Diagnostics.MakeFoundSpecDiagnostic(spec.ClassName, spec.Fields.Count));
                    var generatedFileContents = CreateSourceText(diagnostics, assemblyName, additional, spec);
                    if (!string.IsNullOrWhiteSpace(generatedFileContents.Contents))
                    {
                        results.Add(generatedFileContents);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            diagnostics.Add(Diagnostics.MakeExceptionOccurredDiagnostic(ex));
        }

        return results;
    }

    private static GeneratedFile CreateSourceText(List<Diagnostic> diagnostics, string assemblyName, AdditionalText file, TargetClassSpecification spec)
    {
        try
        {
            var ns = spec.Namespace;
            try
            {
                if (string.IsNullOrWhiteSpace(ns))
                    ns = GuessNamespace(assemblyName, file);
            }
            catch
            {
                ns = spec.Namespace;
                diagnostics.Add(Diagnostics.MakeCouldNotDetermineNamespaceDiagnostic(ns));
            }

            var source = GenerateSourceText(spec, ns);
            var filename = $"{spec.ClassName}.g.cs";
            if (!string.IsNullOrWhiteSpace(spec.FileName))
            {
                filename = spec.FileName.Trim();
            }
            diagnostics.Add(Diagnostics.MakeGeneratedFileDiagnostic(filename, ns));

            return new GeneratedFile {  Contents = source, FileName = filename };
        }
        catch (Exception ex)
        {
            diagnostics.Add(Diagnostics.MakeExceptionOccurredDiagnostic(ex));
            return new GeneratedFile();
        }
    }

    /// <summary>
    /// Try and guess the namespace based on the path of the .oraudt file. Nasty.
    /// </summary>
    private  static string GuessNamespace(string assemblyName, AdditionalText file)
    {
        // Start with the full path of the additional file and trim off the filename.
        var path = Path.GetDirectoryName(file.Path);

        // See if that is somewhere under our assembly. If the project is using
        // normal C# namespaces this should work.

        if (assemblyName != null && path.Contains(assemblyName))
        {
            var idx = path.LastIndexOf(assemblyName);
            path = path.Remove(0, idx);
        }

        // Convert path strings to (hopefully) something like a namespace.
        path = path.Replace('/', '.');
        path = path.Replace('\\', '.');

        return path;
    }

    private static string GenerateSourceText(TargetClassSpecification spec, string ns)
    {
        var sb = new IndentingStringBuilder();
        sb.AppendLines(
            "using System;",
            "using System.Diagnostics;",
            "using Oracle.ManagedDataAccess.Client;",
            "using Oracle.ManagedDataAccess.Types;");

        sb.AppendLine();
        sb.AppendLine($"namespace {ns}");
        using (sb.BeginCodeBlock())
        {
            GenerateClassSource(sb, spec);
            sb.AppendLine();
            GenerateClassFactorySource(sb, spec);
            sb.AppendLine();
            GenerateCollectionSource(sb, spec);
            sb.AppendLine();
            GenerateCollectionFactorySource(sb, spec);
        }

        return sb.ToString();
    }

    private static void GenerateClassSource(IndentingStringBuilder sb, TargetClassSpecification spec)
    {
        if (!string.IsNullOrWhiteSpace(spec.DebuggerDisplayFormat))
            sb.AppendLine($"[DebuggerDisplay(\"{spec.DebuggerDisplayFormat}\")]");

        sb.AppendLine($"public partial class {spec.ClassName} : IOracleCustomType, INullable");
        using (sb.BeginCodeBlock())
        {
            foreach (var field in spec.Fields)
            {
                sb.AppendLines(
                    $"[OracleObjectMapping(\"{field.EffectiveOracleName}\")]",
                    $"public {field.EffectiveTypeName} {field.PropertyName} {{ get; set; }}");
                sb.AppendLine();
            }

            sb.AppendLines(
                "private bool objectIsNull;",
                "public bool IsNull => objectIsNull;",
                $"public static {spec.ClassName} Null => new() {{ objectIsNull = true }};");
            if (!string.IsNullOrWhiteSpace(spec.ToStringFormat))
                sb.AppendLine($"public override string ToString() => $\"{spec.ToStringFormat}\";");

            sb.AppendLine();

            sb.AppendLine("public void FromCustomObject(OracleConnection con, object udt)");
            using (sb.BeginCodeBlock())
            {
                bool first = true;
                foreach (var field in spec.Fields)
                {
                    if (first)
                        first = false;
                    else
                        sb.AppendLine();

                    if (field.IsNonNullable)
                    {
                        sb.AppendLine($"OracleUdt.SetValue(con, udt, \"{field.EffectiveOracleName}\", {field.PropertyName});");
                    }
                    else
                    {
                        // This is probably unnecessary DBNull.Value should be the default.
                        sb.AppendLines(
                            $"if ({field.PropertyName} == null)",
                            $"    OracleUdt.SetValue(con, udt, \"{field.EffectiveOracleName}\", DBNull.Value);",
                            "else",
                            $"    OracleUdt.SetValue(con, udt, \"{field.EffectiveOracleName}\", {field.PropertyName});");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("public void ToCustomObject(OracleConnection con, object udt)");
            using (sb.BeginCodeBlock())
            {
                bool first = true;
                foreach (var field in spec.Fields)
                {
                    if (first)
                        first = false;
                    else
                        sb.AppendLine();

                    if (field.IsNonNullable)
                    {
                        sb.AppendLine($"{field.PropertyName} = ({field.EffectiveTypeName})OracleUdt.GetValue(con, udt,  \"{field.EffectiveOracleName}\");");
                    }
                    else
                    {
                        sb.AppendLines(
                            $"if (OracleUdt.IsDBNull(con, udt, \"{field.EffectiveOracleName}\"))",
                            $"    {field.PropertyName} = null;",
                            "else",
                            $"    {field.PropertyName} = ({field.EffectiveTypeName})OracleUdt.GetValue(con, udt,  \"{field.EffectiveOracleName}\");");
                    }
                }
            }
        }
    }

    private static void GenerateClassFactorySource(IndentingStringBuilder sb, TargetClassSpecification spec)
    {
        sb.AppendLines(
            "/// <summary>",
            $"/// An Oracle factory for the {spec.ClassName} type.",
            "/// This allows us to bind/create individual objects.",
            "/// </summary>",
            $"[OracleCustomTypeMapping(\"{spec.OracleRecordTypeName}\")]",
            $"public partial class {spec.ClassName}Factory: IOracleCustomTypeFactory");

        using (sb.BeginCodeBlock())
        {
            sb.AppendLine("public IOracleCustomType CreateObject()");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"return new {spec.ClassName}();");
            }
        }
    }

    private static void GenerateCollectionSource(IndentingStringBuilder sb, TargetClassSpecification spec)
    {
        if (string.IsNullOrWhiteSpace(spec.CollectionName))
        {
            return;
        }

        sb.AppendLine($"public partial class {spec.CollectionName}: IOracleCustomType, INullable");
        using (sb.BeginCodeBlock())
        {
            sb.AppendLine("[OracleArrayMapping]");
            sb.AppendLine($"public {spec.ClassName}[] Rows;");
            sb.AppendLine();
            sb.AppendLines(
                "private bool objectIsNull;",
                "public bool IsNull => objectIsNull;",
                $"public static {spec.CollectionName} Null => new() {{ objectIsNull = true }};");
            sb.AppendLine();

            sb.AppendLine("public void FromCustomObject(OracleConnection con, object udt)");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine("OracleUdt.SetValue(con, udt, 0, Rows);");
            }
            sb.AppendLine();

            sb.AppendLine("public void ToCustomObject(OracleConnection con, object udt)");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"Rows = ({spec.ClassName}[])OracleUdt.GetValue(con, udt, 0);");
            }
        }
    }

    private static void GenerateCollectionFactorySource(IndentingStringBuilder sb, TargetClassSpecification spec)
    {
        if (string.IsNullOrWhiteSpace(spec.CollectionName))
        {
            return;
        }

        sb.AppendLines(
            "/// <summary>",
            $"/// An Oracle factory for the {spec.CollectionName} type.",
            $"/// This allows us to bind/create arrays of {spec.ClassName} objects.",
            "/// </summary>",
            $"[OracleCustomTypeMapping(\"{spec.OracleCollectionTypeName}\")]",
            $"public partial class {spec.CollectionName}Factory: IOracleCustomTypeFactory, IOracleArrayTypeFactory");

        using (sb.BeginCodeBlock())
        {
            sb.AppendLine($"public IOracleCustomType CreateObject()");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"return new {spec.CollectionName}();");
            }
            sb.AppendLine();

            sb.AppendLine($"public Array CreateArray(int numElems)");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"return new {spec.CollectionName}[numElems];");
            }
            sb.AppendLine();

            sb.AppendLine($"public Array CreateStatusArray(int numElems)");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine("return null;");
            }
        }
    }
}
