using System.Diagnostics;
using ReQuesty.Runtime.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
/// Replaces a portion of the URL.
/// </summary>
/// <typeparam name="TUriReplacementHandlerOption">A type with the rules used to perform a URI replacement.</typeparam>
/// <param name="uriReplacement">An object with the URI replacement rules.</param>
public class UriReplacementHandler<TUriReplacementHandlerOption>(TUriReplacementHandlerOption? uriReplacement = default) : DelegatingHandler where TUriReplacementHandlerOption : IUriReplacementHandlerOption
{
    private readonly TUriReplacementHandlerOption? _uriReplacement = uriReplacement;

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        TUriReplacementHandlerOption? uriReplacement = request.GetRequestOption<TUriReplacementHandlerOption>() ?? _uriReplacement;

        // If there is no URI replacement to apply, then just skip this handler.
        if (uriReplacement is null)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        Activity? activity;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource.StartActivity($"{nameof(UriReplacementHandler<TUriReplacementHandlerOption>)}_{nameof(SendAsync)}");
            activity?.SetTag("requesty.runtime.handler.uri_replacement.enable", uriReplacement.IsEnabled());
        }
        else
        {
            activity = null;
        }

        try
        {
            request.RequestUri = uriReplacement.Replace(request.RequestUri);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            activity?.Dispose();
        }
    }
}
