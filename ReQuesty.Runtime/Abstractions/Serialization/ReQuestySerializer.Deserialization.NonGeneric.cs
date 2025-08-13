using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ReQuesty.Runtime.Abstractions.Serialization;

internal interface IReQuestyDeserializationWrapper
{
    Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken);
    Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken);
    Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken);
    Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken);
}

internal class ReQuestyDeserializationWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : IReQuestyDeserializationWrapper where T : IParsable
{
    public async Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken) => await ReQuestySerializer.DeserializeAsync<T>(contentType, stream, cancellationToken).ConfigureAwait(false);
    public async Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken) => await ReQuestySerializer.DeserializeAsync<T>(contentType, serializedRepresentation, cancellationToken).ConfigureAwait(false);
    public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken) => (await ReQuestySerializer.DeserializeCollectionAsync<T>(contentType, stream, cancellationToken).ConfigureAwait(false)).OfType<IParsable>();
    public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken) => (await ReQuestySerializer.DeserializeCollectionAsync<T>(contentType, serializedRepresentation, cancellationToken).ConfigureAwait(false)).OfType<IParsable>();
}
static internal class ReQuestyDeserializationWrapperFactory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIParsable(Type type) => typeof(IParsable).IsAssignableFrom(type);

    private static readonly ConcurrentDictionary<Type, IReQuestyDeserializationWrapper> _deserializers = new();

    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    static internal IReQuestyDeserializationWrapper Create(Type type) => IsIParsable(type) ? _deserializers.GetOrAdd(type, CreateInternal) : throw new ArgumentException("The given Type is not of IParsable", nameof(type));

    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    private static IReQuestyDeserializationWrapper CreateInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType)
    {
        if (Activator.CreateInstance(typeof(ReQuestyDeserializationWrapper<>).MakeGenericType(targetType)) is IReQuestyDeserializationWrapper deserializer)
        {
            return deserializer;
        }
        else
        {
            throw new InvalidOperationException($"Unable to create deserializer for type {targetType}");
        }
    }
}

public static partial class ReQuestySerializer
{
    /// <summary>
    ///   Deserializes the given string into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>

    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)

        => ReQuestyDeserializationWrapperFactory.Create(type).DeserializeAsync(contentType, serializedRepresentation, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)

         => ReQuestyDeserializationWrapperFactory.Create(type).DeserializeAsync(contentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
         => ReQuestyDeserializationWrapperFactory.Create(type).DeserializeCollectionAsync(contentType, stream, cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => ReQuestyDeserializationWrapperFactory.Create(type).DeserializeCollectionAsync(contentType, serializedRepresentation, cancellationToken);
}