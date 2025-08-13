using System.Diagnostics;
using ReQuesty.Runtime.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
///   The Headers Inspection Handler allows the developer to inspect the headers of the request and response.
/// </summary>
/// <param name="defaultOptions">Default options to apply to the handler</param>
public class HeadersInspectionHandler(HeadersInspectionHandlerOption? defaultOptions = null) : DelegatingHandler
{
    private readonly HeadersInspectionHandlerOption _defaultOptions = defaultOptions ?? new HeadersInspectionHandlerOption();

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        HeadersInspectionHandlerOption options = request.GetRequestOption<HeadersInspectionHandlerOption>() ?? _defaultOptions;

        Activity? activity;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource?.StartActivity($"{nameof(HeadersInspectionHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("requesty.runtime.handler.headersInspection.enable", true);
        }
        else
        {
            activity = null;
        }
        try
        {
            if (options.InspectRequestHeaders)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
                {
                    options.RequestHeaders[header.Key] = ConvertHeaderValuesToArray(header.Value);
                }
                if (request.Content != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> contentHeaders in request.Content.Headers)
                    {
                        options.RequestHeaders[contentHeaders.Key] = ConvertHeaderValuesToArray(contentHeaders.Value);
                    }
                }
            }
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (options.InspectResponseHeaders)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                {
                    options.ResponseHeaders[header.Key] = ConvertHeaderValuesToArray(header.Value);
                }
                if (response.Content != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> contentHeaders in response.Content.Headers)
                    {
                        options.ResponseHeaders[contentHeaders.Key] = ConvertHeaderValuesToArray(contentHeaders.Value);
                    }
                }
            }
            return response;
        }
        finally
        {
            activity?.Dispose();
        }

        static string[] ConvertHeaderValuesToArray(IEnumerable<string> headerValues)
        {
            List<string> headerValuesList = [.. headerValues];
            return headerValuesList.ToArray();
        }
    }
}
