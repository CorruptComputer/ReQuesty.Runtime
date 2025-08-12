using ReQuesty.Runtime.Http.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
///   A <see cref="TelemetryHandler"/> implementation using standard .NET libraries.
/// </summary>
/// <param name="telemetryHandlerOption">The <see cref="TelemetryHandlerOption"/> instance to configure the telemetry</param>
public class TelemetryHandler(TelemetryHandlerOption? telemetryHandlerOption = null) : DelegatingHandler
{
    private readonly TelemetryHandlerOption _telemetryHandlerOption = telemetryHandlerOption ?? new TelemetryHandlerOption();

    /// <summary>
    ///   Send a HTTP request
    /// </summary>
    /// <param name="request">The HTTP request<see cref="HttpRequestMessage"/>needs to be sent.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        TelemetryHandlerOption telemetryHandlerOption = request.GetRequestOption<TelemetryHandlerOption>() ?? _telemetryHandlerOption;

        // use the enriched request from the handler
        if (telemetryHandlerOption.TelemetryConfigurator != null)
        {
            HttpRequestMessage enrichedRequest = telemetryHandlerOption.TelemetryConfigurator(request);
            return await base.SendAsync(enrichedRequest, cancellationToken).ConfigureAwait(false);
        }

        // Just forward the request if TelemetryConfigurator was intentionally set to null
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
