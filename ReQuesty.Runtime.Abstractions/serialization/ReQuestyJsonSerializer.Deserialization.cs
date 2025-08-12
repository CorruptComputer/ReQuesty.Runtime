using System.Diagnostics.CodeAnalysis;

namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Set of helper methods for JSON serialization
/// </summary>
public static partial class ReQuestyJsonSerializer
{
    private const string _jsonContentType = "application/json";

    /// <summary>
    ///   Deserializes the given stream into an object.
    /// </summary>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<T>(string serializedRepresentation, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeAsync(_jsonContentType, serializedRepresentation, parsableFactory, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into an object.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<T>(Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeAsync(_jsonContentType, stream, parsableFactory, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into an object.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeAsync<T>(_jsonContentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into an object.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeAsync<T>(_jsonContentType, serializedRepresentation, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeCollectionAsync(_jsonContentType, stream, parsableFactory, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the objects.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string serializedRepresentation, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeCollectionAsync(_jsonContentType, serializedRepresentation, parsableFactory, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeCollectionAsync<T>(_jsonContentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
        => ReQuestySerializer.DeserializeCollectionAsync<T>(_jsonContentType, serializedRepresentation, cancellationToken);
}