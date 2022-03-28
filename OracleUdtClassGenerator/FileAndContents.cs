using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

public struct FileAndContents
{
    public string AssemblyName { get; set; }
    public AdditionalText AdditionalText { get; set; }
    public string SourceContents { get; set; }
}
