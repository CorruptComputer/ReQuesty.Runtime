namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Represents an untyped node without the value.
/// </summary>
public class UntypedNull : UntypedNode
{
    /// <summary>
    ///   Gets the value associated with untyped null node.
    /// </summary>
    /// <returns>The value associated with untyped null node.</returns>
    public new object? GetValue() => null;
}
