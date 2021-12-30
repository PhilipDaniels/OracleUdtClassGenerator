using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OracleUdtClassGenerator;

/// <summary>
/// The IndentingStringBuilder is a wrapper around the standard StringBuilder
/// that automatically adds indentation when you add a new line. The amount
/// of indentation is setup upon construction.
/// </summary>
internal class IndentingStringBuilder : IDisposable
{
    const string DefaultIndentation = "    ";
    StringBuilder sb;
    readonly string Indentation;
    bool CurrentLineIsEmpty { get; set; }

    /// <summary>
    /// Creates a new IndentingStringBuilder using the default indentation (4 spaces).
    /// </summary>
    public IndentingStringBuilder()
        : this(DefaultIndentation)
    {
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder using the specified indentation.
    /// </summary>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder();
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified capacity and the default indentation (4 spaces).
    /// </summary>
    /// <param name="capacity">Initial string builder capacity.</param>
    public IndentingStringBuilder(int capacity)
        : this(capacity, DefaultIndentation)
    {
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified capacity and indentation.
    /// </summary>
    /// <param name="capacity">Initial string builder capacity.</param>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(int capacity, string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder(capacity);
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with an initial value and indentation.
    /// </summary>
    /// <param name="value">Initial value for the string builder.</param>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(string value, string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder(value);
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified capacity and maximum capacity.
    /// </summary>
    /// <param name="capacity">Initial string builder capacity.</param>
    /// <param name="maxCapacity">Maximum allowed string builder capacity.</param>
    public IndentingStringBuilder(int capacity, int maxCapacity)
        : this(capacity, maxCapacity, DefaultIndentation)
    {
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified capacity, maximum capacity and indentation.
    /// </summary>
    /// <param name="capacity">Initial string builder capacity.</param>
    /// <param name="maxCapacity">Maximum allowed string builder capacity.</param>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(int capacity, int maxCapacity, string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder(capacity, maxCapacity);
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified initial value and capacity.
    /// </summary>
    /// <param name="value">Initial value for the string builder.</param>
    /// <param name="capacity">Initial string builder capacity.</param>
    public IndentingStringBuilder(string value, int capacity)
        : this(value, capacity, DefaultIndentation)
    {
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified initial value, capacity and indentation.
    /// </summary>
    /// <param name="value">Initial value for the string builder.</param>
    /// <param name="capacity">Initial string builder capacity.</param>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(string value, int capacity, string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder(value, capacity);
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified substring, capacity and default indentation.
    /// </summary>
    /// <param name="value">String to take substring of.</param>
    /// <param name="startIndex">Where to start the substring.</param>
    /// <param name="length">How many characters to take.</param>
    /// <param name="capacity">Initial string builder capacity.</param>
    public IndentingStringBuilder(string value, int startIndex, int length, int capacity)
        : this(value, startIndex, length, capacity, DefaultIndentation)
    {
    }

    /// <summary>
    /// Creates a new IndentingStringBuilder with the specified substring, capacity and indentation.
    /// </summary>
    /// <param name="value">String to take substring of.</param>
    /// <param name="startIndex">Where to start the substring.</param>
    /// <param name="length">How many characters to take.</param>
    /// <param name="capacity">Initial string builder capacity.</param>
    /// <param name="indentation">The string to use for indentation.</param>
    public IndentingStringBuilder(string value, int startIndex, int length, int capacity, string indentation)
    {
        Indentation = indentation; // Guard.Argument(indentation, nameof(indentation)).NotNull().Value;
        sb = new StringBuilder(value, startIndex, length, capacity);
        CurrentLineIsEmpty = true;
    }

    /// <summary>
    /// Gets or set the IndentationLevel (a number from 0..N).
    /// </summary>
    public int IndentationLevel
    {
        get
        {
            return _IndentationLevel;
        }
        set
        {
            _IndentationLevel = value; // Guard.Argument(value, "IndentationLevel").Min(0);
        }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int _IndentationLevel;

    int _codeBlockLevel;



    /// <summary>
    /// Begins a code block by writing a "{" and increases the indentation level.
    /// The code block will be automatically closed when the instance is disposed.
    /// </summary>
    /// <returns>Builder for further use. Normally you want to call <c>Dispose</c> on this to
    /// get automatic outdenting.</returns>
    public IndentingStringBuilder BeginCodeBlock()
    {
        AppendLine("{");
        _codeBlockLevel += 1;
        _IndentationLevel += 1;
        return this;
    }

    /// <summary>
    /// Reduce the current indentation level.
    /// If the builder was writing a code block, writes a terminating "}" after decreasing
    /// the indentation level.
    /// </summary>
    public void Dispose()
    {
        bool currentlyInCodeBlock = _codeBlockLevel > 0;
        if (_IndentationLevel > 0)
            _IndentationLevel--;
        if (_codeBlockLevel > 0)
            _codeBlockLevel--;

        if (currentlyInCodeBlock)
        {
            AppendLine("}");
        }
    }

    /// <summary>
    /// Increases the indentation level by 1.
    /// Note that <seealso cref="BeginCodeBlock"/> will manage this automatically in
    /// most circumstances. However, this is useful if you want to manage the indentation
    /// within a block or other type of formatted text.
    /// </summary>
    /// <returns>The builder, for further use.</returns>
    public IndentingStringBuilder Indent()
    {
        IndentationLevel++;
        return this;
    }

    /// <summary>
    /// Decreases the indentation level by 1.
    /// Note that <seealso cref="BeginCodeBlock"/> will manage this automatically in
    /// most circumstances. However, this is useful if you want to manage the indentation
    /// within a block or other type of formatted text.
    /// </summary>
    /// <returns>The builder, for further use.</returns>
    public IndentingStringBuilder OutDent()
    {
        IndentationLevel--;
        return this;
    }

    /// <summary>
    /// Converts the value of this instance to a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return sb.ToString();
    }

    /// <summary>
    /// Gets or sets the maximum number of characters that can be contained in the
    /// memory allocated by the current instance.
    /// </summary>
    public int Capacity
    {
        get
        {
            return sb.Capacity;
        }
        set
        {
            sb.Capacity = value;
        }
    }

    /// <summary>
    /// Gets or sets the length of the current IndentingStringBuilder object.
    /// </summary>
    public int Length
    {
        get
        {
            return sb.Length;
        }
        set
        {
            sb.Length = value;
        }
    }

    /// <summary>
    /// Gets the maximum capacity of this instance.
    /// </summary>
    public int MaxCapacity
    {
        get
        {
            return sb.MaxCapacity;
        }
    }

    /// <summary>
    /// Gets or sets the character at the specified character position in this instance.
    /// </summary>
    /// <param name="index">The position of the character.</param>
    /// <returns>The Unicode character at position index.</returns>
    public char this[int index]
    {
        get
        {
            return sb[index];
        }
        set
        {
            sb[index] = value;
        }
    }

    /// <summary>
    /// Appends the string representation of a specified Boolean value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(bool value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Byte value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(byte value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Char value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(char value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of the Unicode characters in a specified
    /// array to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(char[] value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Decimal value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(decimal value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Double value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(double value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Float value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(float value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Int32 value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(int value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Int64 value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(long value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(object value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified Int16 value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(short value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified sbyte value to this instance.
    /// This is a VB type, and is not CLSCompliant.
    /// The System.Text.StringBuilder class is the same.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(sbyte value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified uint value to this instance.
    /// This is a VB type, and is not CLSCompliant.
    /// The System.Text.StringBuilder class is the same.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(uint value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified ulong value to this instance.
    /// This is a VB type, and is not CLSCompliant.
    /// The System.Text.StringBuilder class is the same.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(ulong value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string representation of a specified ushort value to this instance.
    /// This is a VB type, and is not CLSCompliant.
    /// The System.Text.StringBuilder class is the same.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(ushort value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string value to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder Append(string value)
    {
        AppendIndentationIfNecessary();
        sb.Append(value);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the default line terminator to the end of the current object.
    /// </summary>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendLine()
    {
        AppendIndentationIfNecessary();
        sb.AppendLine();
        CurrentLineIsEmpty = true;
        return this;
    }

    /// <summary>
    /// Appends the string value to this instance and then appends the default line terminator.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendLine(string value)
    {
        AppendIndentationIfNecessary();
        sb.AppendLine(value);
        CurrentLineIsEmpty = true;
        return this;
    }

    /// <summary>
    /// Appends multiple lines, each with the required indentation.
    /// </summary>
    /// <param name="values">List of lines to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendLines(params string[] values)
    {
        if (values != null)
        {
            AppendLines(values.AsEnumerable());
        }

        return this;
    }

    /// <summary>
    /// Appends multiple lines, each with the required indentation.
    /// </summary>
    /// <param name="values">List of lines to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendLines(IEnumerable<string> values)
    {
        if (values != null)
        {
            foreach (var value in values)
            {
                AppendIndentationIfNecessary();
                sb.AppendLine(value);
                CurrentLineIsEmpty = true;
            }

            CurrentLineIsEmpty = true;
        }

        return this;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which
    /// contains zero or more format items, to this instance. Each format item is
    /// replaced by the string representation of a single argument.
    /// </summary>
    /// <param name="format">A format string.</param>
    /// <param name="arg0">An object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendFormat(string format, object arg0)
    {
        AppendIndentationIfNecessary();
        sb.AppendFormat(format, arg0);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which
    /// contains zero or more format items, to this instance. Each format item is
    /// replaced by the string representation of either of two arguments.
    /// </summary>
    /// <param name="format">A format string.</param>
    /// <param name="arg0">An object to format.</param>
    /// <param name="arg1">An object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendFormat(string format, object arg0, object arg1)
    {
        AppendIndentationIfNecessary();
        sb.AppendFormat(format, arg0, arg1);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which
    /// contains zero or more format items, to this instance. Each format item is
    /// replaced by the string representation of either of three arguments.
    /// </summary>
    /// <param name="format">A format string.</param>
    /// <param name="arg0">An object to format.</param>
    /// <param name="arg1">An object to format.</param>
    /// <param name="arg2">An object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
    {
        AppendIndentationIfNecessary();
        sb.AppendFormat(format, arg0, arg1, arg2);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more
    /// format items, to this instance. Each format item is replaced by the string representation of a
    /// corresponding argument in a parameter array.
    /// </summary>
    /// <param name="format">A format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendFormat(string format, params object[] args)
    {
        AppendIndentationIfNecessary();
        AppendFormat(CultureInfo.InvariantCulture, format, args);
        CurrentLineIsEmpty = false;
        return this;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more
    /// format items, to this instance. Each format item is replaced by the string representation of a
    /// corresponding argument in a parameter array.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public IndentingStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
    {
        AppendIndentationIfNecessary();
        sb.AppendFormat(provider, format, args);
        CurrentLineIsEmpty = false;
        return this;
    }

    void AppendIndentationIfNecessary()
    {
        if (!CurrentLineIsEmpty)
            return;

        CurrentLineIsEmpty = false;

        for (int i = 0; i < IndentationLevel; i++)
            sb.Append(Indentation);
    }
}
