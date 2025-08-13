using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization;

/// <summary>
///   Extension methods for <see cref="IParsable"/> instances.
/// </summary>
public static class ParsableExtensions
{
    /// <summary>
    ///   Serializes the given object into a stream based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object t </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(this T value, string contentType, bool serializeOnlyChangedValues = false) where T : IParsable
        => ReQuestySerializer.SerializeAsStream(contentType, value, serializeOnlyChangedValues);


    /// <summary>
    ///   Serializes the given object into a string based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsStringAsync<T>(this T value, string contentType, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
        => await ReQuestySerializer.SerializeAsStringAsync(contentType, value, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    ///   Serializes the given collection into a stream based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object t </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(this IEnumerable<T> value, string contentType, bool serializeOnlyChangedValues = false) where T : IParsable
        => ReQuestySerializer.SerializeAsStream(contentType, value, serializeOnlyChangedValues);

    /// <summary>
    ///   Serializes the given collection into a string based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsStringAsync<T>(this IEnumerable<T> value, string contentType, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
        => await ReQuestySerializer.SerializeAsStringAsync(contentType, value, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    ///   Serializes the given collection into a stream based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object t </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(this T[] value, string contentType, bool serializeOnlyChangedValues = false) where T : IParsable
        => ReQuestySerializer.SerializeAsStream(contentType, value, serializeOnlyChangedValues);

    /// <summary>
    ///   Serializes the given collection into a string based on the content type.
    /// </summary>
    /// <param name="contentType">Content type to serialize the object to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="ReQuesty.Runtime.Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsStringAsync<T>(this T[] value, string contentType, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
        => await ReQuestySerializer.SerializeAsStringAsync(contentType, value, serializeOnlyChangedValues, cancellationToken);
}