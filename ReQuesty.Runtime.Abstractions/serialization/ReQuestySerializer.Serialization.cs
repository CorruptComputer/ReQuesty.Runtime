using System.ComponentModel;

namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Set of helper methods for serialization
/// </summary>
public static partial class ReQuestySerializer
{
    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, T value, bool serializeOnlyChangedValues = true) where T : IParsable
    {
        using ISerializationWriter writer = GetSerializationWriter(contentType, value, serializeOnlyChangedValues);
        writer.WriteObjectValue(null, value);
        Stream stream = writer.GetSerializedContent();
        return stream;
    }

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Task<string> SerializeAsStringAsync<T>(string contentType, T value, CancellationToken cancellationToken) where T : IParsable => SerializeAsStringAsync(contentType, value, true, cancellationToken);

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(string contentType, T value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
    {
        using Stream stream = SerializeAsStream(contentType, value, serializeOnlyChangedValues);
        return GetStringFromStreamAsync(stream, cancellationToken);
    }

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default, you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, IEnumerable<T> value, bool serializeOnlyChangedValues = true) where T : IParsable
    {
        using ISerializationWriter writer = GetSerializationWriter(contentType, value, serializeOnlyChangedValues);
        writer.WriteCollectionOfObjectValues(null, value);
        Stream stream = writer.GetSerializedContent();
        return stream;
    }

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues"></param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(string contentType, IEnumerable<T> value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
    {
        using Stream stream = SerializeAsStream(contentType, value, serializeOnlyChangedValues);
        return GetStringFromStreamAsync(stream, cancellationToken);
    }

    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Task<string> SerializeAsStringAsync<T>(string contentType, IEnumerable<T> value, CancellationToken cancellationToken) where T : IParsable => SerializeAsStringAsync(contentType, value, true, cancellationToken);

    private static async Task<string> GetStringFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ISerializationWriter GetSerializationWriter(string contentType, object value, bool serializeOnlyChangedValues = true)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        ArgumentNullException.ThrowIfNull(value);
        return SerializationWriterFactoryRegistry.DefaultInstance.GetSerializationWriter(contentType, serializeOnlyChangedValues);
    }
}