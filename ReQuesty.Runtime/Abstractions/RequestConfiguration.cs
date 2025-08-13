namespace ReQuesty.Runtime.Abstractions;

/// <summary>
///   Request configuration type for the request builders
/// </summary>
/// <typeparam name="T">The type of the query parameters class.</typeparam>
public class RequestConfiguration<T> where T : class, new()
{
    /// <summary>
    ///   Request headers
    /// </summary>
    public RequestHeaders Headers { get; set; } = [];

    /// <summary>
    ///   Request options
    /// </summary>
    public IList<IRequestOption> Options { get; set; } = [];

    /// <summary>
    ///   Query parameters
    /// </summary>
    public T QueryParameters { get; set; } = new();
}