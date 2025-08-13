namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Represents an untyped node with decimal value.
/// </summary>
/// <param name="value">The decimal value associated with the node.</param>
public class UntypedDecimal(decimal value) : UntypedNode
{
    private readonly decimal _value = value;

    /// <summary>
    ///   Gets the value associated with untyped decimal node.
    /// </summary>
    /// <returns>The value associated with untyped decimal node.</returns>
    public new decimal GetValue() => _value;
}
