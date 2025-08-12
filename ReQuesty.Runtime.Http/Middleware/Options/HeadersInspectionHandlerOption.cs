using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Http.Middleware.Options;

/// <summary>
///   The Headers Inspection Option allows the developer to inspect the headers of the request and response.
/// </summary>
public class HeadersInspectionHandlerOption : IRequestOption
{
    /// <summary>
    ///   Gets or sets a value indicating whether the request headers should be inspected.
    /// </summary>
    public bool InspectRequestHeaders
    {
        get; set;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the response headers should be inspected.
    /// </summary>
    public bool InspectResponseHeaders
    {
        get; set;
    }

    /// <summary>
    ///   Gets the request headers to for the current request.
    /// </summary>
    public RequestHeaders RequestHeaders { get; private set; } = [];

    /// <summary>
    ///   Gets the response headers for the current request.
    /// </summary>
    public RequestHeaders ResponseHeaders { get; private set; } = [];
}
