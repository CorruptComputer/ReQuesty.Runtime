using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Form;

/// <summary>
///   The <see cref="IAsyncParseNodeFactory"/> implementation for form content types
/// </summary>
public class FormParseNodeFactory : IAsyncParseNodeFactory
{
    /// <inheritdoc/>
    public string ValidContentType => "application/x-www-form-urlencoded";

    /// <inheritdoc/>
    public IParseNode GetRootParseNode(string contentType, Stream content)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        if (!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        }

        ArgumentNullException.ThrowIfNull(content);

        using StreamReader reader = new(content);
        string rawValue = reader.ReadToEnd();
        return new FormParseNode(rawValue);
    }

    /// <inheritdoc/>
    public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        if (!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        }

        ArgumentNullException.ThrowIfNull(content);

        using StreamReader reader = new(content);
        string rawValue = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        return new FormParseNode(rawValue);
    }
}
