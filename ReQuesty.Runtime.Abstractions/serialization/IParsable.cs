namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Defines a serializable model object.
/// </summary>
/// <remarks>In the ReQuesty.Runtime.Serialization namespace, you can find extension methods for serializing this object.</remarks>
public interface IParsable
{
    /// <summary>
    ///   Gets the deserialization information for this object.
    /// </summary>
    /// <returns>The deserialization information for this object where each entry is a property key with its deserialization callback.</returns>
    IDictionary<string, Action<IParseNode>> GetFieldDeserializers();

    /// <summary>
    ///   Writes the objects properties to the current writer.
    /// </summary>
    /// <param name="writer">The <see cref="ISerializationWriter">writer</see> to write to.</param>
    void Serialize(ISerializationWriter writer);
}
