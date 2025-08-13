using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ReQuesty.Runtime.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
///   The Body Inspection Handler allows the developer to inspect the body of the request and response.
/// </summary>
/// <param name="defaultOptions">Default options to apply to the handler</param>
public class BodyInspectionHandler(BodyInspectionHandlerOption? defaultOptions = null) : DelegatingHandler
{
    private readonly BodyInspectionHandlerOption _defaultOptions = defaultOptions ?? new BodyInspectionHandlerOption();

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        BodyInspectionHandlerOption options = request.GetRequestOption<BodyInspectionHandlerOption>() ?? _defaultOptions;

        Activity? activity;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(
                obsOptions.TracerInstrumentationName
            );
            activity = activitySource?.StartActivity(
                $"{nameof(BodyInspectionHandler)}_{nameof(SendAsync)}"
            );
            activity?.SetTag("requesty.runtime.handler.bodyInspection.enable", true);
        }
        else
        {
            activity = null;
        }
        try
        {
            if (options.InspectRequestBody)
            {
                options.RequestBody = await CopyToStreamAsync(request.Content, cancellationToken)
                    .ConfigureAwait(false);
            }
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (options.InspectResponseBody)
            {
                options.ResponseBody = await CopyToStreamAsync(response.Content, cancellationToken)
                    .ConfigureAwait(false);
            }

            return response;
        }
        finally
        {
            activity?.Dispose();
        }

        [return: NotNullIfNotNull(nameof(httpContent))]
        static async Task<Stream> CopyToStreamAsync(
            HttpContent? httpContent,
            CancellationToken cancellationToken
        )
        {
            if (httpContent is null or { Headers.ContentLength: 0 })
            {
                return Stream.Null;
            }

            MemoryStream stream = new();
            await httpContent.LoadIntoBufferAsync().ConfigureAwait(false);

            await httpContent.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return stream;
        }
    }
}
