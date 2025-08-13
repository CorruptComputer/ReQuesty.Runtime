using System.Diagnostics.CodeAnalysis;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Abstractions.Store;

namespace ReQuesty.Runtime.Abstractions;

/// <summary>
///   Service responsible for translating abstract Request Info into concrete native HTTP requests.
/// </summary>
public interface IRequestAdapter
{
    /// <summary>
    ///   Enables the backing store proxies for the SerializationWriters and ParseNodes in use.
    /// </summary>
    /// <param name="backingStoreFactory">The backing store factory to use.</param>
    void EnableBackingStore(IBackingStoreFactory backingStoreFactory);

    /// <summary>
    ///   Gets the serialization writer factory currently in use for the HTTP core service.
    /// </summary>
    ISerializationWriterFactory SerializationWriterFactory { get; }

    /// <summary>
    ///   Executes the HTTP request specified by the given RequestInformation and returns the deserialized response model.
    /// </summary>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="factory">The factory of the response model to deserialize the response into.</param>
    /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>The deserialized response model.</returns>
    Task<ModelType?> SendAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default) where ModelType : IParsable;

    /// <summary>
    ///   Executes the HTTP request specified by the given RequestInformation and returns the deserialized response model collection.
    /// </summary>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="factory">The factory of the response model to deserialize the response into.</param>
    /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>The deserialized response model collection.</returns>
    Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default) where ModelType : IParsable;

    /// <summary>
    ///   Executes the HTTP request specified by the given RequestInformation and returns the deserialized primitive response model.
    /// </summary>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>The deserialized primitive response model.</returns>
    Task<ModelType?> SendPrimitiveAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default);

    /// <summary>
    ///   Executes the HTTP request specified by the given RequestInformation and returns the deserialized primitive response model collection.
    /// </summary>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>The deserialized primitive response model collection.</returns>
    Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default);

    /// <summary>
    ///   Executes the HTTP request specified by the given RequestInformation with no return content.
    /// </summary>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>A Task to await completion.</returns>
    Task SendNoContentAsync(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default);

    /// <summary>
    ///   The base url for every request.
    /// </summary>
    string? BaseUrl { get; set; }

    /// <summary>
    ///   Converts the given RequestInformation into a native HTTP request used by the implementing adapter.
    /// </summary>
    /// <typeparam name="T">The type of the native request.</typeparam>
    /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the requests.</param>
    /// <returns>The native HTTP request.</returns>
    Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default);
}
