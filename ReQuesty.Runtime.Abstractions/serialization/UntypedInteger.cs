﻿namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Represents an untyped node with integer value.
/// </summary>
/// <param name="value">The integer value associated with the node.</param>
public class UntypedInteger(int value) : UntypedNode
{
    private readonly int _value = value;

    /// <summary>
    ///   Gets the value associated with untyped integer node.
    /// </summary>
    /// <returns>The value associated with untyped integer node.</returns>
    public new int GetValue() => _value;
}
