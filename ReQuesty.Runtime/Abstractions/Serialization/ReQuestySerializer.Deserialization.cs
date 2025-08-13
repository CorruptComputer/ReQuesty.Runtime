using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ReQuesty.Runtime.Abstractions.Serialization;

public static partial class ReQuestySerializer
{
    private static ParsableFactory<T> GetFactoryFromType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() where T : IParsable
    {
        Type type = typeof(T);
        System.Reflection.MethodInfo factoryMethod = Array.Find(type.GetMethods(), static x => x.IsStatic && "CreateFromDiscriminatorValue".Equals(x.Name, StringComparison.OrdinalIgnoreCase)) ??
                            throw new InvalidOperationException($"No factory method found for type {type.Name}");
        return (ParsableFactory<T>)factoryMethod.CreateDelegate(typeof(ParsableFactory<T>));
    }

    /// <summary>
    ///   Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<T?> DeserializeAsync<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory,
        CancellationToken cancellationToken = default) where T : IParsable
    {
        if (string.IsNullOrEmpty(serializedRepresentation))
        {
            throw new ArgumentNullException(nameof(serializedRepresentation));
        }

        using Stream stream = await GetStreamFromStringAsync(serializedRepresentation).ConfigureAwait(false);
        return await DeserializeAsync(contentType, stream, parsableFactory, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Stream> GetStreamFromStringAsync(string source)
    {
        MemoryStream stream = new();
        using StreamWriter writer = new(stream, Encoding.UTF8, 1024, true);

        // Some clients enforce async stream processing.
        await writer.WriteAsync(source).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    ///   Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<T?> DeserializeAsync<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory,
        CancellationToken cancellationToken = default) where T : IParsable
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(parsableFactory);
        IParseNode parseNode = await ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(contentType, stream, cancellationToken).ConfigureAwait(false);
        return parseNode.GetObjectValue(parsableFactory);
    }

    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
        => DeserializeAsync(contentType, stream, GetFactoryFromType<T>(), cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
        => DeserializeAsync(contentType, serializedRepresentation, GetFactoryFromType<T>(), cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(parsableFactory);
        IParseNode parseNode = await ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(contentType, stream, cancellationToken);
        return parseNode.GetCollectionOfObjectValues(parsableFactory);
    }

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the objects.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, string serializedRepresentation,
        ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    {
        if (string.IsNullOrEmpty(serializedRepresentation))
        {
            throw new ArgumentNullException(nameof(serializedRepresentation));
        }

        using Stream stream = await GetStreamFromStringAsync(serializedRepresentation).ConfigureAwait(false);
        return await DeserializeCollectionAsync(contentType, stream, parsableFactory, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
        => DeserializeCollectionAsync(contentType, stream, GetFactoryFromType<T>(), cancellationToken);

    /// <summary>
    ///   Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
        => DeserializeCollectionAsync(contentType, serializedRepresentation, GetFactoryFromType<T>(), cancellationToken);
}