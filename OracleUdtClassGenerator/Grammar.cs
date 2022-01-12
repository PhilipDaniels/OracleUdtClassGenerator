using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace OracleUdtClassGenerator;

public static class Grammar
{
    public static List<TargetClassSpecification> ParseTargetSpecs(string input)
    {
        input = input.Trim();
        var specs = TargetClassSpecificationListParser.Parse(input);
        return specs;
    }

    static readonly Parser<char> SeparatorChar = Parse.Chars("()<>@,;:\\\"/[]={} \t");
    static readonly Parser<char> ControlChar = Parse.Char(char.IsControl, "Control character");
    static readonly Parser<char> TokenChar =
        Parse.AnyChar
            .Except(SeparatorChar)
            .Except(ControlChar);

    static readonly Parser<char> DoubleQuote = Parse.Char('"');
    static readonly Parser<char> Backslash = Parse.Char('\\');
    static readonly Parser<char> QdText = Parse.AnyChar.Except(DoubleQuote);
    static readonly Parser<char> QuotedPair =
        from _ in Backslash
        from c in Parse.AnyChar
        select c;

    static readonly Parser<string> QuotedString =
        from open in DoubleQuote
        from text in QuotedPair.Or(QdText).Many().Text()
        from close in DoubleQuote
        select text;

    static readonly Parser<string> OptionalCommaParser =
        from _ in Parse.String(",").TokenOnLine().Optional()
        select "";

    static readonly Parser<(string, string)> ClassNameParser =
        from _ in Parse.IgnoreCase("CLASS").TokenOnLine()
        from csharpClassName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from oracleObjectTypeName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from _2 in OptionalCommaParser
        select (csharpClassName, oracleObjectTypeName);

    static readonly Parser<(string, string)> CollectionNameParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _ in Parse.IgnoreCase("COLLECTION").TokenOnLine()
        from csharpCollectionName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from oracleCollectionTypeName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from _2 in OptionalCommaParser
        select (csharpCollectionName, oracleCollectionTypeName);

    static readonly Parser<string> NamespaceParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _ in Parse.IgnoreCase("NAMESPACE").TokenOnLine()
        from name in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from _2 in OptionalCommaParser
        select name;

    static readonly Parser<string> FilenameParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _ in Parse.IgnoreCase("FILENAME").TokenOnLine()
        from name in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from _2 in OptionalCommaParser
        select name;

    static readonly Parser<string> DebugParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _ in Parse.IgnoreCase("DEBUGGERDISPLAY").TokenOnLine()
        from ddspec in QuotedString
        from _2 in OptionalCommaParser
        select ddspec;

    static readonly Parser<string> ToStringParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _ in Parse.IgnoreCase("TOSTRING").TokenOnLine()
        from ddspec in QuotedString
        from _2 in OptionalCommaParser
        select ddspec;

    static readonly Parser<string> FieldKeywordParser =
        from _ws1 in Parse.WhiteSpace.Many()
        from _f in Parse.IgnoreCase("FIELDS").Token().Text()
        from _lb in Parse.String("[").Token().Text()
        select "";

    static readonly Parser<FieldSpecification> ThreePartFieldParser =
        from typeName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from propertyName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from oracleName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        select new FieldSpecification
        {
            DotNetDataTypeName = typeName,
            PropertyName = propertyName,
            OracleFieldName = oracleName,
        };

    static readonly Parser<FieldSpecification> TwoPartFieldParser =
        from typeName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        from propertyName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        select new FieldSpecification
        {
            DotNetDataTypeName = typeName,
            PropertyName = propertyName,
        };

    static readonly Parser<FieldSpecification> OnePartFieldParser =
        from propertyName in Parse.Identifier(Parse.Letter, TokenChar).TokenOnLine().Text()
        select new FieldSpecification
        {
            PropertyName = propertyName,
        };

    // Most consuming parser first.
    static readonly Parser<FieldSpecification> FieldParser =
        ThreePartFieldParser
        .Or(TwoPartFieldParser)
        .Or(OnePartFieldParser);

    static readonly Parser<string> FieldSeparatorParser =
        from _ in Parse.LineEnd.Or(Parse.String(",").Token())
        select "";

    static readonly Parser<List<FieldSpecification>> FieldListParser =
        from open in FieldKeywordParser
        from elements in FieldParser.DelimitedBy(FieldSeparatorParser)
        from _ws1 in Parse.WhiteSpace.Many()
        from close in Parse.Char(']')
        from _ws2 in Parse.WhiteSpace.Many()
        select elements.ToList();

    static readonly Parser<TargetClassSpecification> TargetClassSpecificationParser =
        from classNames in ClassNameParser
        from collectionNames in CollectionNameParser.Optional()
        from namespaceName in NamespaceParser.Optional()
        from filename in FilenameParser.Optional()
        from ddspec in DebugParser.Optional()
        from tsspec in ToStringParser.Optional()
        from fields in FieldListParser
        select new TargetClassSpecification
        {
            FileName = filename.GetOrDefault(),
            Namespace = namespaceName.GetOrDefault(),
            ClassName = classNames.Item1,
            CollectionName = collectionNames.IsDefined ? collectionNames.Get().Item1 : null,
            OracleRecordTypeName = classNames.Item2,
            OracleCollectionTypeName = collectionNames.IsDefined ? collectionNames.Get().Item2 : null,
            DebuggerDisplayFormat = ddspec.GetOrDefault(),
            ToStringFormat = tsspec.GetOrDefault(),
            Fields = fields
        };

    static readonly Parser<List<TargetClassSpecification>> TargetClassSpecificationListParser =
        from t in TargetClassSpecificationParser.Many()
        select t.ToList();

    /// <summary>
    /// Works like the built-in 'Token' method, but doesn't go onto new lines.
    /// Handy for languages like this one where newlines are significant.
    /// </summary>
    static Parser<T> TokenOnLine<T>(this Parser<T> parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException("parser");
        }

        return from leading in Parse.Chars(" \t").Many()
               from item in parser
               from trailing in Parse.Chars(" \t").Many()
               select item;
    }
}
