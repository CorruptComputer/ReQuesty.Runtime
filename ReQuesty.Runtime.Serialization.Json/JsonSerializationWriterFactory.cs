using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Json;

/// <summary>
///   The <see cref="ISerializationWriterFactory"/> implementation for the json content type
/// </summary>
/// <param name="reQuestyJsonSerializationContext">The ReQuestyJsonSerializationContext to use.</param>
public class JsonSerializationWriterFactory(ReQuestyJsonSerializationContext reQuestyJsonSerializationContext) : ISerializationWriterFactory
{
    private readonly ReQuestyJsonSerializationContext _reQuestyJsonSerializationContext = reQuestyJsonSerializationContext;

    /// <summary>
    ///   The <see cref="JsonSerializationWriterFactory"/> constructor.
    /// </summary>
    public JsonSerializationWriterFactory()
        : this(ReQuestyJsonSerializationContext.Default) { }

    /// <summary>
    ///   The valid content type for json
    /// </summary>
    public string ValidContentType { get; } = "application/json";

    /// <summary>
    ///   Get a valid <see cref="ISerializationWriter"/> for the content type
    /// </summary>
    /// <param name="contentType">The content type to search for</param>
    /// <returns>A <see cref="ISerializationWriter"/> instance for json writing</returns>
    public ISerializationWriter GetSerializationWriter(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }
        else if (!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        }

        return new JsonSerializationWriter(_reQuestyJsonSerializationContext);
    }
}
