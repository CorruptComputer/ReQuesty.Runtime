namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Represents an untyped node with float value.
/// </summary>
/// <param name="value">The float value associated with the node.</param>
public class UntypedFloat(float value) : UntypedNode
{
    private readonly float _value = value;

    /// <summary>
    ///   Gets the value associated with untyped float node.
    /// </summary>
    /// <returns>The value associated with untyped float node.</returns>
    public new float GetValue() => _value;
}
