using System.Collections.Generic;

namespace OracleUdtClassGenerator;

public class TargetClassSpecification
{
    public string Namespace { get; set; }
    public string ClassName { get; set; }
    public string OracleRecordTypeName { get; set; }
    public string OracleCollectionTypeName { get; set; }
    public string DebuggerDisplayFormat { get; set; }
    public string ToStringFormat { get; set; }
    public List<FieldSpecification> Fields { get; set; } = new List<FieldSpecification>();
}

public class FieldSpecification
{
    public string PropertyName { get; set; }
    public string DotNetDataTypeName { get; set; }
    public string OracleFieldName { get; set; }

    public string EffectiveOracleName
    {
        get
        {
            return (OracleFieldName ?? PropertyName).ToUpperInvariant();
        }
    }

    public string EffectiveTypeName
    {
        get
        {
            return DotNetDataTypeName ?? "string";
        }
    }

    static HashSet<string> NonNullableTypes = new HashSet<string>()
    {
        "bool",
        "short",
        "int",
        "long",
        "decimal",
        "float",
        "double",
        "datetime",
        "system.boolean",
        "system.int16",
        "system.int32",
        "system.int64",
        "system.decimal",
        "system.single",
        "system.double",
        "system.datetime"
    };

    public bool IsNonNullable
    {
        get
        {
            var t = EffectiveTypeName.ToLowerInvariant();
            if (t.Contains("?"))
            {
                return false;
            }
            else
            {
                return NonNullableTypes.Contains(t);
            }
        }
    }
}
