using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Multipart;

/// <summary>
///   Factory to create multipart serialization writers.
/// </summary>
public class MultipartSerializationWriterFactory : ISerializationWriterFactory
{
    /// <inheritdoc/>
    public string ValidContentType => "multipart/form-data";

    /// <inheritdoc/>
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

        return new MultipartSerializationWriter();
    }
}
