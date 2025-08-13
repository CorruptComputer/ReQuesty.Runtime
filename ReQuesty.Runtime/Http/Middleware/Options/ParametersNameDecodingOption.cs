using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Http.Middleware.Options;

/// <summary>
///   The ParametersEncodingOption request class
/// </summary>
public class ParametersNameDecodingOption : IRequestOption
{
    /// <summary>
    ///   Whether to decode the specified characters in the request query parameters names
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///   The list of characters to decode in the request query parameters names before executing the request
    /// </summary>
    public List<char> ParametersToDecode { get; set; } = ['$']; // '.', '-', '~' already being decoded by Uri
}
