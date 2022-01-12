# Oracle UDT Class Generator

[![NuGet Badge](https://buildstats.info/nuget/oracleudtclassgenerator)](https://www.nuget.org/packages/OracleUdtClassGenerator/)

This NuGet package contains a source generator that emits C# source code
for mapping to and from
[Oracle User-Defined Data Types](https://docs.oracle.com/en/database/oracle/oracle-data-access-components/19.3.2/odpnt/featUDTs.html#GUID-7913CDD0-CB22-4257-828F-FBCCA3FE9126).
This allows your project to easily send records and arrays of records to and from an
Oracle database.

Simply install the NuGet package into the project in which you wish to generate
mapping classes. The generator runs entirely at compile time and does not add
any runtime dependencies to your project.

The emitted code has no dependencies other than 
[Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/)
which you must manually add to the client project.

For examples of using UDTs please refer to the 'OracleExamples' in my
GitHub solution [BassUtils](https://www.github.com/PhilipDaniels/BassUtils)
and its corresponding
[NuGet Package](https://www.nuget.org/packages/BassUtils.Oracle)


# 'oraudt' Input Format

The C# code necessary to map UDTs is quite long winded and error-prone,
and this package greatly simplifies the process by using a domain-specific
language (DSL) which allows very concise specification of the Oracle UDT.

1. Create a file ending in `.oraudt`, for example `Person.oraudt`.
2. Set the file's build action to `C# analyzer additional file`.
3. Write a specification for your UDT.

The specification consists of a series of sections. First, the class specification,
which is mandatory:
```
class PersonRecord MYSCHEMA.objPerson
```

* The first identifier, `PersonRecord` is the name of the C# class to be
  generated.
* The second identifier, `MYSCHEMA.objPerson` is the name of the Oracle
  UDT type that you created in the database using "CREATE TYPE objPerson
  AS OBJECT".

Then the collection specification, which is optional:
```
class PersonRecord MYSCHEMA.objPerson
    Collection PersonArray MYSCHEMA.tblPerson
```

Including the `Collection` specification makes the generator create a
second type, in this case `PersonArray`, and a corresponding factory
class, together they make it possible to bind **arrays** of `PersonRecord`,
not just individual records. In other words, they make it possible to
use table types that are created using the syntax "CREATE TYPE tblPerson
AS TABLE OF objPerson". This is the Oracle equivalent to using Table-Valued
Parameters in MS SQL Server.

Then come 4 lines which are all optional BUT HAVE TO BE IN THIS ORDER:

```
class PersonRecord MYSCHEMA.objPerson
    Collection PersonArray MYSCHEMA.tblPerson
    Namespace People
    Filename Person.g.cs
    DebuggerDisplay "{Age}/{Height}"
    ToString "{Age}/{Height}/{Role}/{Salary}"
```

Namespace specifies the exact namespace to generate the code in.
If absent, the generator tries to guess the namespace based on the
folder that the `.oraudt` file is in.

`Filename` allows you to control the name of the file used by the
generated code. Normally this is defaulted based on the name of the
class (PersonRecord.g.cs in this case) but sometimes this automatic
algorithm may lead to name clashes, for example if you have many similar
record types in one project but in different namespaces. `Filename`
allows you to override the automatic generation to avoid this overwriting.

`DebuggerDisplay` and `ToString` can be used to automatically generate
a `DebuggerDisplayAttribute` and a `ToString` method, respectively.
This is only intended for simple usages, for more complex requirements,
leave them blank and write your own implementations - this is easy
because **all the code generated is in partial classes**.

Finally we come to the field list specification. This section is
mandatory and must contain at least one field.

```
    Fields [
          FirstName, LastName,
          int Age
          string EducationLevel EDUCLEVEL
    ]
```

It consists of the word "Fields" and then a list of fields inside
"[]" brackets, and separated either by commas or newlines.

An individual field specification can have 1 to 3 parts. A 3 part
name is the most powerful; if you use a two part name or a 1 part
name then conventions are used to fill in this missing parts.

An example of a 3 part name is

```
decimal? Width WDT
```

Here `decimal?` is the C# type of the property, `Width` is the C#
name of the property, and `WDT` is the name of the field in the
Oracle `OBJECT` type (it must always be uppercase).

A 2 part name omits the last component and defaults it to the name
of the field - in the following example, it will be `WEIGHT`:

```
decimal? Weight
```

Finally, a 1 part name allows you to omit the C# type name which causes it
to be defaulted to `string`:

```
FirstName
```

So this last field specification is equivalent to

```
string FirstName FIRSTNAME
```

This can allow for very succinct object definitions as string fields
are very common.

## Full Examples

A simple `Person` class:

```
class PersonRecord MYSCHEMA.objPerson
    Collection PersonArray MYSCHEMA.tblPerson
    Namespace People
    Fields [int Age,FirstName,LastName]
```

A more expansive example:

```
class ArticleRecord MYSCHEMA.objArticleRecord
    Collection ArticleArray MYSCHEMA.tblArticleRecord
    Namespace MyDatabaseProject.UDTs
    DebuggerDisplay "{Sku}"
    ToString "{Sku}, {Cost}, {Description}"
    Fields [
        Sku
        decimal? Cost
        string Description DESC
        int? MyNullableProp SOMENULLABLEPROP
        JustSomeString
        int AnIntegerProperty
        decimal? ThreeParter SOMEORACLENAME
        SingleParter
    ]
```

## Notes

A single `.oraudt` file can contain multiple `class` stanzas. Each will
be generated into a separate C# file.

Bool fields do not map well to Oracle types since Oracle SQL has no
concept of a boolean type. Typically a NUMBER(1) or a CHAR(1) is used.
Therefore I suggest using the partial class facility to create a 
boolean property which proxies the mapped field.

All the generated files will be in a single virtual folder, therefore
all your class names must be distinct across your entire project. Use
the `Filename` clause to avoid clashes.

The keywords 'class', 'collection', 'namespace' etc. are all case-insensitive.

# Output Format

Two classes will be generated for each `.oraudt` file, or four if a
Collection is specified. Given a class name of `Person`,
a class `Person` will be created containing all the fields, and a
class `PersonFactory` which is required by Oracle Data Acccess to
instantiate the `Person`.

If you specify the collection type then a `PersonArray` type will also be
generated, plus a factory for **that**, called `PersonArrayFactory`.

All the classes are partial classes.

The generated code can be seen under the `Analyzers` node in Visual Studio.

**n.b.** Visual Studio can be temperamental about showing these files.
Sometimes it is necessary to close and reopen Visual Studio.

If all goes well you will also have a log file that looks something like this:

```cs
/*
DateTime.UtcNow: 2022-01-01T22:33:03.5397481Z

Found file C:\repos\OracleUdtClassGenerator\OracleUdtClassGenerator.ConsoleTestHarness\ArticleMaster.oraudt
  Found spec for ArticleMasterRecord with 17 fields
  Generated file ArticleMasterRecord.g.cs in namespace OracleUdtClassGenerator.ConsoleTestHarness
Found file C:\repos\OracleUdtClassGenerator\OracleUdtClassGenerator.ConsoleTestHarness\MyClass.oraudt
  Found spec for MyClass with 5 fields
  Generated file MyClass.g.cs in namespace MyNamespace
Found file C:\repos\OracleUdtClassGenerator\OracleUdtClassGenerator.ConsoleTestHarness\SubFolder\ArticleMaster2.oraudt
  Found spec for ArticleMasterRecord2 with 17 fields
  Generated file ArticleMasterRecord2.g.cs in namespace OracleUdtClassGenerator.ConsoleTestHarness.SubFolder
Found file C:\repos\OracleUdtClassGenerator\OracleUdtClassGenerator.ConsoleTestHarness\Person.oraudt
  Found spec for PersonRecord with 3 fields
  Generated file PersonRecord.g.cs in namespace People
*/
```

# Further Reading

An article describing the implementation of this source generator
is available
[on my blog](https://www.philipdaniels.com/blog/2022/oracle-udt-class-generator/).

I have another NuGet package available called
[BassUtils.Oracle](https://www.nuget.org/packages/BassUtils.Oracle)
which can help with calling procs with UDTs, for example it will create
`OracleParameters` correctly for you. It is also described in an
[article on my blog](https://www.philipdaniels.com/blog/2021/oracle-from-csharp/).
