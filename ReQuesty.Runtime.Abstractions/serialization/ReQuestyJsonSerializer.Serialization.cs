namespace ReQuesty.Runtime.Abstractions.Serialization;

public static partial class ReQuestyJsonSerializer
{
    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(T value, bool serializeOnlyChangedValues = true) where T : IParsable
        => ReQuestySerializer.SerializeAsStream(_jsonContentType, value, serializeOnlyChangedValues);

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(T value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.SerializeAsStringAsync(_jsonContentType, value, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(IEnumerable<T> value, bool serializeOnlyChangedValues = true) where T : IParsable
        => ReQuestySerializer.SerializeAsStream(_jsonContentType, value, serializeOnlyChangedValues);

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(IEnumerable<T> value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.SerializeAsStringAsync(_jsonContentType, value, serializeOnlyChangedValues, cancellationToken);
}
