using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Http.Middleware.Options
{
    /// <summary>
    /// The Telemetry request option class
    /// </summary>
    public class TelemetryHandlerOption : IRequestOption
    {
        /// <summary>
        /// A delegate that's called to configure the <see cref="HttpRequestMessage"/> with the appropriate telemetry values.
        /// </summary>
        public Func<HttpRequestMessage, HttpRequestMessage> TelemetryConfigurator { get; set; } = (request) => request;
    }
}
