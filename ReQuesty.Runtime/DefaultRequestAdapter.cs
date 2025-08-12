using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Http;
using ReQuesty.Runtime.Serialization.Form;
using ReQuesty.Runtime.Serialization.Json;
using ReQuesty.Runtime.Serialization.Multipart;
using ReQuesty.Runtime.Serialization.Text;

namespace ReQuesty.Runtime;

/// <summary>
///   The <see cref="IRequestAdapter"/> implementation that derived from <see cref="HttpClientRequestAdapter"/> with registrations configured.
/// </summary>
public class DefaultRequestAdapter : HttpClientRequestAdapter
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="DefaultRequestAdapter"/> class.
    /// </summary>
    /// <param name="authenticationProvider">The authentication provider.</param>
    /// <param name="parseNodeFactory">The parse node factory.</param>
    /// <param name="serializationWriterFactory">The serialization writer factory.</param>
    /// <param name="httpClient">The native HTTP client.</param>
    /// <param name="observabilityOptions">The observability options.</param>
    public DefaultRequestAdapter(IAuthenticationProvider authenticationProvider, IAsyncParseNodeFactory? parseNodeFactory = null, ISerializationWriterFactory? serializationWriterFactory = null, HttpClient? httpClient = null, ObservabilityOptions? observabilityOptions = null) : base(authenticationProvider, parseNodeFactory, serializationWriterFactory, httpClient, observabilityOptions)
    {
        SetupDefaults();
    }

    private static void SetupDefaults()
    {
        // Setup the default serializers/deserializers
        ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
        ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
        ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
        ApiClientBuilder.RegisterDefaultSerializer<MultipartSerializationWriterFactory>();
        ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
        ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
        ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
    }
}
