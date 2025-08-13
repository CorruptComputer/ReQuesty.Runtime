using System.Text.Json;
using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Json;

/// <summary>
///   The <see cref="IAsyncParseNodeFactory"/> implementation for json content types
/// </summary>
/// <param name="jsonJsonSerializationContext">The ReQuestySerializationContext to utilize.</param>
public class JsonParseNodeFactory(ReQuestyJsonSerializationContext jsonJsonSerializationContext) : IAsyncParseNodeFactory
{
    private readonly ReQuestyJsonSerializationContext _jsonJsonSerializationContext = jsonJsonSerializationContext;

    /// <summary>
    ///   The <see cref="JsonParseNodeFactory"/> constructor.
    /// </summary>
    public JsonParseNodeFactory()
        : this(ReQuestyJsonSerializationContext.Default) { }

    /// <summary>
    ///   The valid content type for json
    /// </summary>
    public string ValidContentType { get; } = "application/json";

    /// <summary>
    ///   Asynchronously gets the root <see cref="IParseNode"/> of the json to be read.
    /// </summary>
    /// <param name="contentType">The content type of the stream to be parsed</param>
    /// <param name="content">The <see cref="Stream"/> containing json to parse.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns>An instance of <see cref="IParseNode"/> for json manipulation</returns>
    public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }
        else if (!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        }

        _ = content ?? throw new ArgumentNullException(nameof(content));

        using JsonDocument jsonDocument = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
        return new JsonParseNode(jsonDocument.RootElement.Clone(), _jsonJsonSerializationContext);
    }
}
