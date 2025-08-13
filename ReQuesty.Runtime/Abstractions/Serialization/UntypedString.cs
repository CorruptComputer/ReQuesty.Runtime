﻿namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Represents an untyped node with string value.
/// </summary>
/// <param name="value">The string value associated with the node.</param>
public class UntypedString(string? value) : UntypedNode
{
    private readonly string? _value = value;

    /// <summary>
    ///   Gets the string associated with untyped string node.
    /// </summary>
    /// <returns>The string associated with untyped string node.</returns>
    public new string? GetValue() => _value;
}
