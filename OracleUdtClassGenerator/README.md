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
language which allows very concise specification of the Oracle UDT.

1. Create a file ending in `.oraudt`, for example `Person.oraudt`.
2. Set the file's build action to `C# analyzer additional file`.
3. Write a DSL specification for your UDT.

The DSL consists of a series of sections. First, the class specification:

```
class PersonRecord MYSCHEMA.objPerson MYSCHEMA.tblPerson
```

* The first identifier, `PersonRecord` is the name of the C# class to be
  generated. It's mandatory.
* The second identifier, `MYSCHEMA.objPerson` is the name of the Oracle
  UDT type that you created in the database using "CREATE TYPE objPerson
  AS OBJECT". This is also mandatory.
* The third identifier, "MYSCHEMA.tblPerson" is the name of the Oracle UDT nested
  table type that you created, for example "CREATE TYPE tblPerson AS TABLE OF objPerson".
  This field is optional. If it is not present, then C# classes for mapping
  arrays of records will not be generated.

Then come 3 lines which are all optional:

```
class PersonRecord MYSCHEMA.objPerson MYSCHEMA.tblPerson
    Namespace People
    DebuggerDisplay "{Sku}/{Price}"
    ToString "{Sku}/{Color}/{Size}/{Price}"
```

Namespace specifies the exact namespace to generate the code in.
If absent, the generator tries to guess the namespace based on the
folder that the `.oraudt` file is in. The string `.OracleUDTs` will
always be appended to that namespace.

`DebuggerDisplay` and `ToString` can be used to automatically generate
a `DebuggerDisplayAttribute` and a `ToString` method, respectively.
This is only intended for simple usages, for more complex requirements,
leave them blank and write your own implementations - this is easy
because **all the code generated is in partial classes**.

Finally we come to the field list specification. This section is
mandatory:

```
    Fields [
          FirstName,
          LastName
          int Age
          string EducationLevel EDUCLEVEL
    ]
```

It consists of the word "Fields" and then a list of fields inside
"[]" brackets, and separated either by commas or newlines.

An individual field specification can have 1 to 3 parts. A 3 part
name is the most flexible; if you use a two part name or a 1 part
name then conventions are used to fill in this missing parts.

An example of a 3 part name is

```
decimal? Width WDTH
```

Here `decimal?` is the C# type of the property, `Width` is the C#
name of the property, and `WDTH` is the name of the field in the
Oracle `OBJECT` type.

A 2 part name omits the last component and defaults it to the name
of the field - in the following example, it will be `WEIGHT` (it must
always be uppercase).

```
decimal? Weight
```

Finally, 1 part name allows you to omit the C# type name which causes it
to be defaulted to `string`

```
FirstName
```

So this last field is equivalent to

```
string FirstName FIRSTNAME
```

This can allow for very succinct object definitions as string fields
are very common.

## Full Examples

A simple `Person` class:

```
class PersonRecord MYSCHEMA.objPerson MYSCHEMA.tblPerson
    Namespace People
    Fields [int Age,FirstName,LastName]
```

A more expansive example:

```
class MyClass MYSCHEMA.objArticleRecord MYSCHEMA.tblArticleRecord
    Namespace MyNamespace
    DebuggerDisplay "{Sku}"
    ToString "Hello world"
    Fields [
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
all your class names must be distinct across your entire project.

# Output Format

Two classes will be generated for each `.oraudt` file, or four if the
Oracle table type is specified. Given a class name of `Person`,
a class `Person` will be created containing all the fields, and a
class `PersonFactory` which is required by Oracle Data Acccess to
instantiate the `Person`.

If you specify the table type then a `PersonArray` type will also be
generated, plus a factory for **that**, called `PersonArrayFactory`.
The names of all these classes are derived from the class name and
cannot be overridden. They are all partial classes.

The generated code can be seen under the `Analyzers` node in Visual Studio.

**n.b.** Visual Studio can be temperamental about showing these files.
Sometimes it is necessary to close and reopen Visual Studio.


# Further Reading

An article describing the implementation of this source generator
is available
[on my blog](https://www.philipdaniels.com/blog/2022/oracle-udt-class-generator/).

I have another NuGet package available called
[BassUtils.Oracle](https://www.nuget.org/packages/BassUtils.Oracle)
which can help with calling procs with UDTs, for example it will create
`OracleParameters` correctly for you. It is also described in an
[article on my blog](https://www.philipdaniels.com/blog/2021/oracle-from-csharp/).
