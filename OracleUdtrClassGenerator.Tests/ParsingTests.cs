using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OracleUdtClassGenerator.Core.Tests;

[TestClass]
public class ParsingTests
{
    [TestMethod]
    public void WithRecordTypeOnly()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
                Dummy
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithFilename()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
                Filename MyFile.g.cs
            Fields [
                Dummy
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].FileName.Should().Be("MyFile.g.cs");
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndNamespace()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Namespace Some.Name.Space
            Fields [
                Dummy
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().Be("Some.Name.Space");
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndNamespaceAndTrailingComma()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE  ,  
            Namespace Some.Name.Space   ,   
            Fields [
                Dummy
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().Be("Some.Name.Space");
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeOnlyAndDebug()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            DebuggerDisplay ""my_debug_spec {Sku}""
            Fields [
                Dummy
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().Be("my_debug_spec {Sku}");
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeOnlyAndDebugAndToString()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Namespace Some.Name.Space
            Filename MyFile.g.cs
            DebuggerDisplay ""my_debug_spec {Sku}""
            ToString ""format_something""
            Fields [
                Dummy
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().Be("Some.Name.Space");
        specs[0].FileName.Should().Be("MyFile.g.cs");
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().Be("my_debug_spec {Sku}");
        specs[0].ToStringFormat.Should().Be("format_something");
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndCollectionType()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE SCHEMA.COLLECTIONTYPE
            Fields [
                Dummy
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().Be("SCHEMA.COLLECTIONTYPE");
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndCollectionTypeAndDebug()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE SCHEMA.COLLECTIONTYPE
            DebuggerDisplay ""my_debug_spec {Sku}""
            Fields [
                Dummy
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().Be("SCHEMA.COLLECTIONTYPE");
        specs[0].DebuggerDisplayFormat.Should().Be("my_debug_spec {Sku}");
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndCollectionTypeAndDebugAndToString()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE SCHEMA.COLLECTIONTYPE
            DebuggerDisplay ""my_debug_spec {Sku}""
            ToString ""format_something""
            Fields [
                Dummy
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().Be("SCHEMA.COLLECTIONTYPE");
        specs[0].DebuggerDisplayFormat.Should().Be("my_debug_spec {Sku}");
        specs[0].ToStringFormat.Should().Be("format_something");
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void WithRecordTypeAndCollectionTypeAndDebugAndToStringAndTrailingCommas()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE SCHEMA.COLLECTIONTYPE
            DebuggerDisplay ""my_debug_spec {Sku}"",
            ToString ""format_something"",
            Fields [
                Dummy
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().Be("SCHEMA.COLLECTIONTYPE");
        specs[0].DebuggerDisplayFormat.Should().Be("my_debug_spec {Sku}");
        specs[0].ToStringFormat.Should().Be("format_something");
        specs[0].Fields.Count.Should().Be(1);
    }

    [TestMethod]
    public void OneSimpleField()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
                FirstProperty
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(1);
        specs[0].Fields[0].PropertyName.Should().Be("FirstProperty");
        specs[0].Fields[0].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[0].OracleFieldName.Should().BeNull();
    }

    [TestMethod]
    public void ThreeSimpleFields()
    {
        // Test spaces and tabs within the field list.
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
                FirstProperty   
                         SecondProperty      
ThirdProperty   
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(3);

        specs[0].Fields[0].PropertyName.Should().Be("FirstProperty");
        specs[0].Fields[0].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[0].OracleFieldName.Should().BeNull();

        specs[0].Fields[1].PropertyName.Should().Be("SecondProperty");
        specs[0].Fields[1].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[1].OracleFieldName.Should().BeNull();

        specs[0].Fields[2].PropertyName.Should().Be("ThirdProperty");
        specs[0].Fields[2].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[2].OracleFieldName.Should().BeNull();
    }

    [TestMethod]
    public void FieldsAreCorrect()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
System.Int32? NullableProp ORACLENAME
                PropertyOnly
                int PropAndType
      decimal? PropAndTypeAndOracle ORACLENAME2
                OtherPropertyOnly
            ]
        ";

        var specs =  Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(5);

        specs[0].Fields[0].PropertyName.Should().Be("NullableProp");
        specs[0].Fields[0].DotNetDataTypeName.Should().Be("System.Int32?");
        specs[0].Fields[0].OracleFieldName.Should().Be("ORACLENAME");

        specs[0].Fields[1].PropertyName.Should().Be("PropertyOnly");
        specs[0].Fields[1].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[1].OracleFieldName.Should().BeNull();

        specs[0].Fields[2].PropertyName.Should().Be("PropAndType");
        specs[0].Fields[2].DotNetDataTypeName.Should().Be("int");
        specs[0].Fields[2].OracleFieldName.Should().BeNull();

        specs[0].Fields[3].PropertyName.Should().Be("PropAndTypeAndOracle");
        specs[0].Fields[3].DotNetDataTypeName.Should().Be("decimal?");
        specs[0].Fields[3].OracleFieldName.Should().Be("ORACLENAME2");

        specs[0].Fields[4].PropertyName.Should().Be("OtherPropertyOnly");
        specs[0].Fields[4].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[4].OracleFieldName.Should().BeNull();
    }

    [TestMethod]
    public void FieldsAreCorrectUsingCommasAsFieldSeparator()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
System.Int32? NullableProp ORACLENAME ,          PropertyOnly
                int PropAndType
      decimal? PropAndTypeAndOracle ORACLENAME2,OtherPropertyOnly
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(5);

        specs[0].Fields[0].PropertyName.Should().Be("NullableProp");
        specs[0].Fields[0].DotNetDataTypeName.Should().Be("System.Int32?");
        specs[0].Fields[0].OracleFieldName.Should().Be("ORACLENAME");

        specs[0].Fields[1].PropertyName.Should().Be("PropertyOnly");
        specs[0].Fields[1].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[1].OracleFieldName.Should().BeNull();

        specs[0].Fields[2].PropertyName.Should().Be("PropAndType");
        specs[0].Fields[2].DotNetDataTypeName.Should().Be("int");
        specs[0].Fields[2].OracleFieldName.Should().BeNull();

        specs[0].Fields[3].PropertyName.Should().Be("PropAndTypeAndOracle");
        specs[0].Fields[3].DotNetDataTypeName.Should().Be("decimal?");
        specs[0].Fields[3].OracleFieldName.Should().Be("ORACLENAME2");

        specs[0].Fields[4].PropertyName.Should().Be("OtherPropertyOnly");
        specs[0].Fields[4].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[4].OracleFieldName.Should().BeNull();
    }

    [TestMethod]
    public void FieldsAreCorrectUsingCommasAsFieldSeparatorAndAtLineEnd()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
System.Int32? NullableProp ORACLENAME    ,          PropertyOnly,
                int PropAndType   ,
      decimal? PropAndTypeAndOracle ORACLENAME2     ,OtherPropertyOnly
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(1);
        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(5);

        specs[0].Fields[0].PropertyName.Should().Be("NullableProp");
        specs[0].Fields[0].DotNetDataTypeName.Should().Be("System.Int32?");
        specs[0].Fields[0].OracleFieldName.Should().Be("ORACLENAME");

        specs[0].Fields[1].PropertyName.Should().Be("PropertyOnly");
        specs[0].Fields[1].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[1].OracleFieldName.Should().BeNull();

        specs[0].Fields[2].PropertyName.Should().Be("PropAndType");
        specs[0].Fields[2].DotNetDataTypeName.Should().Be("int");
        specs[0].Fields[2].OracleFieldName.Should().BeNull();

        specs[0].Fields[3].PropertyName.Should().Be("PropAndTypeAndOracle");
        specs[0].Fields[3].DotNetDataTypeName.Should().Be("decimal?");
        specs[0].Fields[3].OracleFieldName.Should().Be("ORACLENAME2");

        specs[0].Fields[4].PropertyName.Should().Be("OtherPropertyOnly");
        specs[0].Fields[4].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[4].OracleFieldName.Should().BeNull();
    }

    [TestMethod]
    public void MultipleSpecsInOneFile()
    {
        var input = @"
        class MyClass SCHEMA.RECORDTYPE
            Fields [
System.Int32? NullableProp ORACLENAME
                PropertyOnly
                int PropAndType
      decimal? PropAndTypeAndOracle ORACLENAME2
                OtherPropertyOnly
            ]

        class MyClass2 SCHEMA.RECORDTYPE2
 Namespace Some.Name.Space
        ToString ""format_something""
            Fields [
                PropertyOnly2
                int PropAndType2
            ]
        ";

        var specs = Grammar.ParseTargetSpecs(input);
        specs.Count.Should().Be(2);

        specs[0].Namespace.Should().BeNull();
        specs[0].ClassName.Should().Be("MyClass");
        specs[0].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE");
        specs[0].OracleCollectionTypeName.Should().BeNull();
        specs[0].DebuggerDisplayFormat.Should().BeNull();
        specs[0].ToStringFormat.Should().BeNull();
        specs[0].Fields.Count.Should().Be(5);

        specs[0].Fields[0].PropertyName.Should().Be("NullableProp");
        specs[0].Fields[0].DotNetDataTypeName.Should().Be("System.Int32?");
        specs[0].Fields[0].OracleFieldName.Should().Be("ORACLENAME");

        specs[0].Fields[1].PropertyName.Should().Be("PropertyOnly");
        specs[0].Fields[1].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[1].OracleFieldName.Should().BeNull();

        specs[0].Fields[2].PropertyName.Should().Be("PropAndType");
        specs[0].Fields[2].DotNetDataTypeName.Should().Be("int");
        specs[0].Fields[2].OracleFieldName.Should().BeNull();

        specs[0].Fields[3].PropertyName.Should().Be("PropAndTypeAndOracle");
        specs[0].Fields[3].DotNetDataTypeName.Should().Be("decimal?");
        specs[0].Fields[3].OracleFieldName.Should().Be("ORACLENAME2");

        specs[0].Fields[4].PropertyName.Should().Be("OtherPropertyOnly");
        specs[0].Fields[4].DotNetDataTypeName.Should().BeNull();
        specs[0].Fields[4].OracleFieldName.Should().BeNull();

        specs[1].Namespace.Should().Be("Some.Name.Space");
        specs[1].ClassName.Should().Be("MyClass2");
        specs[1].OracleRecordTypeName.Should().Be("SCHEMA.RECORDTYPE2");
        specs[1].OracleCollectionTypeName.Should().BeNull();
        specs[1].DebuggerDisplayFormat.Should().BeNull();
        specs[1].ToStringFormat.Should().Be("format_something");
        specs[1].Fields.Count.Should().Be(2);

        specs[1].Fields[0].PropertyName.Should().Be("PropertyOnly2");
        specs[1].Fields[0].DotNetDataTypeName.Should().BeNull();
        specs[1].Fields[0].OracleFieldName.Should().BeNull();

        specs[1].Fields[1].PropertyName.Should().Be("PropAndType2");
        specs[1].Fields[1].DotNetDataTypeName.Should().Be("int");
        specs[1].Fields[1].OracleFieldName.Should().BeNull();
    }
}
