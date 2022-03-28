using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OracleUdtClassGenerator;

public class FileAndContents
{
    public AdditionalText AdditionalText { get; set; }
    public string Contents { get; set; }
}

public class GeneratedFile
{
    public string FileName { get; set; } 
    public string Contents { get; set; }
}

[Generator(LanguageNames.CSharp)]
public class OracleUdtGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // nb. See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md#caching

        IncrementalValueProvider<string> assemblyName;
        IncrementalValuesProvider<AdditionalText> texts;
        IncrementalValuesProvider<(AdditionalText Left, string Right)> textsAndAssembly;

        try
        {
            assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
            texts = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".oraudt", StringComparison.OrdinalIgnoreCase));
            textsAndAssembly = texts.Combine(assemblyName);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }

        context.RegisterSourceOutput(textsAndAssembly, (spc, pair) =>
        {

            try
            {
                var text = pair.Left;
                var assemblyName = pair.Right;
                var contents = text.GetText()!.ToString();

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    var generatedFiles = ProcessOraUdtFile(assemblyName, text, contents);
                    foreach (var generatedFile in generatedFiles)
                    {
                        var sourceText = SourceText.From(generatedFile.Contents, Encoding.UTF8);
                        spc.AddSource(generatedFile.FileName, sourceText);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
            finally
            {
                // Also produce a logs file.
                Logger.WriteLogsToFile(spc);
            }
        });

        /*
        // Read the contents of each additional file.
        IncrementalValuesProvider<FileAndContents> namesAndContents = texts
            .Select((addText, cancellationToken) => new FileAndContents {
                AdditionalText = addText,
                Contents = addText.GetText(cancellationToken)!.ToString()
            })
            .Where(files => !string.IsNullOrWhiteSpace(files.Contents));


        context.RegisterSourceOutput(compilationAndUdts, (sourceProductionContext, generationInput) =>
        {
            var comp = generationInput.Item1;
            var inputFiles = generationInput.Item2;

            foreach (var inputFile in inputFiles)
            {
                var generatedFiles = ProcessOraUdtFile(comp, inputFile.AdditionalText, inputFile.Contents);
                foreach (var generatedFile in generatedFiles)
                {
                    var sourceText = SourceText.From(generatedFile.Contents, Encoding.UTF8);
                    sourceProductionContext.AddSource(generatedFile.FileName, sourceText);
                }
            }

            // Also produce a logs file.
            Logger.WriteLogsToFile(sourceProductionContext);
            
        });
        */
    }

    private IEnumerable<GeneratedFile> ProcessOraUdtFile(string assemblyName, AdditionalText additional, string contents)
    {
        var results = new List<GeneratedFile>();

        try
        {
            Logger.Log($"Found file {additional.Path}");

            var targetSpecs = Grammar.ParseTargetSpecs(contents);
            foreach (var spec in targetSpecs)
            {
                Logger.Log($"  Found spec for {spec.ClassName} with {spec.Fields.Count} fields");
                var generatedFileContents = CreateSourceText(assemblyName, additional, spec);
                if (generatedFileContents != null)
                {
                    results.Add(generatedFileContents);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return Enumerable.Empty<GeneratedFile>();
        }
    }

    private GeneratedFile CreateSourceText(string assemblyName, AdditionalText file, TargetClassSpecification spec)
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
                Logger.Log($"  Could not determine namespace from project folder structure, using default of {ns}");
            }

            var source = GenerateSourceText(spec, ns);
            var filename = $"{spec.ClassName}.g.cs";
            if (!string.IsNullOrWhiteSpace(spec.FileName))
            {
                filename = spec.FileName.Trim();
            }
            Logger.Log($"  Generated file {filename} in namespace {ns}");
            return new GeneratedFile {  Contents = source, FileName = filename };
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Try and guess the namespace based on the path of the .oraudt file. Nasty.
    /// </summary>
    private string GuessNamespace(string assemblyName, AdditionalText file)
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

    private string GenerateSourceText(TargetClassSpecification spec, string ns)
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

    private void GenerateClassSource(IndentingStringBuilder sb, TargetClassSpecification spec)
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

    private void GenerateClassFactorySource(IndentingStringBuilder sb, TargetClassSpecification spec)
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

    private void GenerateCollectionSource(IndentingStringBuilder sb, TargetClassSpecification spec)
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

    private void GenerateCollectionFactorySource(IndentingStringBuilder sb, TargetClassSpecification spec)
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
