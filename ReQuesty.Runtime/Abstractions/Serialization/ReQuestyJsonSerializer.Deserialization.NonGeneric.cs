using System.Diagnostics.CodeAnalysis;

namespace ReQuesty.Runtime.Abstractions.Serialization;

public static partial class ReQuestyJsonSerializer
{

    /// <summary>
    ///   Deserializes the given string into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
        => ReQuestySerializer.DeserializeAsync(targetType, _jsonContentType, serializedRepresentation, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, Stream stream, CancellationToken cancellationToken = default)
         => ReQuestySerializer.DeserializeAsync(targetType, _jsonContentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, Stream stream, CancellationToken cancellationToken = default)
         => ReQuestySerializer.DeserializeCollectionAsync(targetType, _jsonContentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => ReQuestySerializer.DeserializeCollectionAsync(targetType, _jsonContentType, serializedRepresentation, cancellationToken);
}
