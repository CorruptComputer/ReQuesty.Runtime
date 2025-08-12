using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Http;

/// <summary>
///   Holds the tracing, metrics and logging configuration for the request adapter
/// </summary>
public class ObservabilityOptions : IRequestOption
{
    /// <summary>
    ///   Gets or sets a value indicating whether to include attributes which could contain EUII information.
    /// </summary>
    public bool IncludeEUIIAttributes { get; set; }

    private static readonly Lazy<string> _name = new(() => typeof(ObservabilityOptions).Namespace!);

    /// <summary>
    ///   Gets the observability name to use for the tracer
    /// </summary>
    public string TracerInstrumentationName => _name.Value;
}
