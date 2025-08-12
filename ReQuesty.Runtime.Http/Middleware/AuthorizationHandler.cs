using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Extensions;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
///   Adds an Authorization header to the request if the header is not already present.
///   Also handles Continuous Access Evaluation (CAE) claims challenges if the initial
///   token request was made using this handler
/// </summary>
/// <param name="authenticationProvider"></param>
/// <exception cref="ArgumentNullException"></exception>
public class AuthorizationHandler(BaseBearerTokenAuthenticationProvider authenticationProvider) : DelegatingHandler
{
    private const string AuthorizationHeader = "Authorization";
    private readonly BaseBearerTokenAuthenticationProvider authenticationProvider = authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));

    /// <summary>
    ///   Adds an Authorization header if not already provided
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Activity? activity = null;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource?.StartActivity($"{nameof(AuthorizationHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("requesty.runtime.handler.authorization.enable", true);
        }

        try
        {
            if (request.Headers.Contains(AuthorizationHeader))
            {
                activity?.SetTag("requesty.runtime.handler.authorization.token_present", true);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            Dictionary<string, object> additionalAuthenticationContext = [];
            await AuthenticateRequestAsync(request, additionalAuthenticationContext, activity, cancellationToken).ConfigureAwait(false);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized || response.RequestMessage == null || !response.RequestMessage.IsBuffered())
            {
                return response;
            }
            // Attempt CAE claims challenge
            string claims = ContinuousAccessEvaluation.GetClaims(response);
            if (string.IsNullOrEmpty(claims))
            {
                return response;
            }

            activity?.AddEvent(new ActivityEvent("requesty.runtime.handler.authorization.challenge_received"));
            additionalAuthenticationContext[ContinuousAccessEvaluation.ClaimsKey] = claims;
            HttpRequestMessage retryRequest = await response.RequestMessage.CloneAsync(cancellationToken);
            await AuthenticateRequestAsync(retryRequest, additionalAuthenticationContext, activity, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("http.request.resend_count", 1);
            return await base.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            activity?.Dispose();
        }
    }

    private async Task AuthenticateRequestAsync(HttpRequestMessage request,
            Dictionary<string, object> additionalAuthenticationContext,
            Activity? activityForAttributes,
            CancellationToken cancellationToken)
    {
        IAccessTokenProvider accessTokenProvider = authenticationProvider.AccessTokenProvider;
        if (request.RequestUri == null || !accessTokenProvider.AllowedHostsValidator.IsUrlHostValid(
                request.RequestUri))
        {
            return;
        }
        string accessToken = await accessTokenProvider.GetAuthorizationTokenAsync(
                request.RequestUri,
                additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);
        activityForAttributes?.SetTag("requesty.runtime.handler.authorization.token_obtained", true);
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
