namespace ReQuesty.Runtime.Abstractions.Authentication;

/// <summary>
///   Provides a base class for implementing <see cref="IAuthenticationProvider" /> for Bearer token scheme.
/// </summary>
/// <param name="accessTokenProvider">The <see cref="IAccessTokenProvider"/> to use for getting the access token.</param>
public class BaseBearerTokenAuthenticationProvider(IAccessTokenProvider accessTokenProvider) : IAuthenticationProvider
{
    /// <summary>
    ///   Gets the <see cref="IAccessTokenProvider" /> to use for getting the access token.
    /// </summary>
    public IAccessTokenProvider AccessTokenProvider { get; private set; } = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
    private const string AuthorizationHeaderKey = "Authorization";
    private const string ClaimsKey = "claims";

    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (additionalAuthenticationContext != null &&
            additionalAuthenticationContext.ContainsKey(ClaimsKey) &&
            request.Headers.ContainsKey(AuthorizationHeaderKey))
        {
            request.Headers.Remove(AuthorizationHeaderKey);
        }

        if (!request.Headers.ContainsKey(AuthorizationHeaderKey))
        {
            string token = await AccessTokenProvider.GetAuthorizationTokenAsync(request.URI, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add(AuthorizationHeaderKey, $"Bearer {token}");
            }
        }
    }
}
