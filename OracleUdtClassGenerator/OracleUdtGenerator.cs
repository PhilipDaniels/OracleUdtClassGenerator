using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OracleUdtClassGenerator;

[Generator]
public class OracleUdtGenerator : ISourceGenerator
{
    private SyntaxReceiver syntaxReceiver = new SyntaxReceiver();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => syntaxReceiver);
    }

    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
#endif

        try
        {
            // Generating as we find each file produces better logs.
            foreach (var file in context.AdditionalFiles)
            {
                if (Path.GetExtension(file.Path).Equals(".oraudt", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log($"Found file {file.Path}");
                    ProcessAdditionalFile(context, file);
                }
            }

        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }

        Logger.WriteLogsToFile(context);
    }

    private void ProcessAdditionalFile(GeneratorExecutionContext context, AdditionalText file)
    {
        try
        {
            var text = file.GetText()?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                ProcessAdditionalFileContents(context, file, text);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    private void ProcessAdditionalFileContents(GeneratorExecutionContext context, AdditionalText file, string text)
    {
        try
        {
            var targetSpecs = Grammar.ParseTargetSpecs(text);
            foreach (var spec in targetSpecs)
            {
                Logger.Log($"  Found spec for {spec.ClassName} with {spec.Fields.Count} fields");
                CreateSourceFile(context, file, spec);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    private void CreateSourceFile(GeneratorExecutionContext context, AdditionalText file, TargetClassSpecification spec)
    {
        try
        {
            var ns = spec.Namespace;
            try
            {
                if (string.IsNullOrWhiteSpace(ns))
                    ns = GuessNamespace(context, file);
            }
            catch
            {
                ns = spec.Namespace;
                Logger.Log($"  Could not determine namespace from project folder structure, using default of {ns}");
            }

            var source = GenerateSourceText(context, spec, ns);
            var filename = $"{spec.ClassName}.g.cs";
            if (!string.IsNullOrWhiteSpace(spec.FileName))
            {
                filename = spec.FileName.Trim();
            }
            Logger.Log($"  Generated file {filename} in namespace {ns}");
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    /// <summary>
    /// Try and guess the namespace based on the path of the .oraudt file. Nasty.
    /// </summary>
    private string GuessNamespace(GeneratorExecutionContext context, AdditionalText file)
    {
        // Start with the full path of the additional file and trim off the filename.
        var path = Path.GetDirectoryName(file.Path);

        // See if that is somewhere under our assembly. If the project is using
        // normal C# namespaces this should work.
        var asmName = context.Compilation?.Assembly?.Name;

        if (asmName != null && path.Contains(asmName))
        {
            var idx = path.LastIndexOf(asmName);
            path = path.Remove(0, idx);
        }

        // Convert path strings to (hopefully) something like a namespace.
        path = path.Replace('/', '.');
        path = path.Replace('\\', '.');

        return path;
    }

    private string GenerateSourceText(GeneratorExecutionContext context, TargetClassSpecification spec, string ns)
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
        if (string.IsNullOrWhiteSpace(spec.OracleCollectionTypeName))
        {
            return;
        }

        sb.AppendLine($"public partial class {spec.ClassName}Array : IOracleCustomType, INullable");
        using (sb.BeginCodeBlock())
        {
            sb.AppendLine("[OracleArrayMapping]");
            sb.AppendLine($"public {spec.ClassName}[] Rows;");
            sb.AppendLine();
            sb.AppendLines(
                "private bool objectIsNull;",
                "public bool IsNull => objectIsNull;",
                $"public static {spec.ClassName}Array Null => new() {{ objectIsNull = true }};");
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
        if (string.IsNullOrWhiteSpace(spec.OracleCollectionTypeName))
        {
            return;
        }

        sb.AppendLines(
            "/// <summary>",
            $"/// An Oracle factory for the {spec.ClassName}Array type.",
            "/// This allows us to bind/create arrays of objects.",
            "/// </summary>",
            $"[OracleCustomTypeMapping(\"{spec.OracleCollectionTypeName}\")]",
            $"public partial class {spec.ClassName}ArrayFactory: IOracleCustomTypeFactory, IOracleArrayTypeFactory");

        using (sb.BeginCodeBlock())
        {
            sb.AppendLine($"public IOracleCustomType CreateObject()");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"return new {spec.ClassName}Array();");
            }
            sb.AppendLine();

            sb.AppendLine($"public Array CreateArray(int numElems)");
            using (sb.BeginCodeBlock())
            {
                sb.AppendLine($"return new {spec.ClassName}Array[numElems];");
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
