using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Http.Middleware.Options;

/// <summary>
///   The User Agent Handler Option request class
/// </summary>
public class UserAgentHandlerOption : IRequestOption
{
    /// <summary>
    ///   Whether to append the ReQuesty version to the user agent header
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///   The product name to append to the user agent header
    /// </summary>
    public string ProductName { get; set; } = "ReQuesty.Runtime";

    /// <summary>
    ///   The product version to append to the user agent header
    /// </summary>
    public string ProductVersion { get; set; } = ReQuesty.Runtime.Http.Generated.Version.Current();
}
