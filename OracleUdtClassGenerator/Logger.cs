using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OracleUdtClassGenerator;

internal static class Logger
{
    internal static List<string> Logs { get; } = new();

    /// <summary>
    /// Adds a new log message.
    /// </summary>
    public static void Log(string msg)
    {
        Logs.Add(msg);
    }

    /// <summary>
    /// Writes the output to the log file.
    /// </summary>
    internal static void WriteLogsToFile(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/*");
        sb.AppendLine($"DateTime.UtcNow: {DateTime.UtcNow:O}");
        sb.AppendLine();
        foreach (var line in Logs)
        {
            sb.AppendLine(line);
        }
        sb.AppendLine("*/");

        context.AddSource("_Logs.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
