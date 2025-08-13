using System.Diagnostics;

namespace ReQuesty.Runtime.Abstractions.Authentication;

/// <summary>
///   This authentication provider authenticates requests using an API key.
/// </summary>
public class ApiKeyAuthenticationProvider : IAuthenticationProvider
{
    private readonly string ApiKey;
    private readonly string ParameterName;
    private readonly KeyLocation KeyLoc;
    private readonly AllowedHostsValidator AllowedHostsValidator;

    /// <summary>
    ///   Instantiates a new <see cref="ApiKeyAuthenticationProvider"/> using the provided parameters.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="parameterName">The name of the query parameter or header to use for authentication.</param>
    /// <param name="keyLocation">The location of the API key.</param>
    /// <param name="allowedHosts">The hosts that are allowed to use the provided API key.</param>
    public ApiKeyAuthenticationProvider(string apiKey, string parameterName, KeyLocation keyLocation, params string[] allowedHosts)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey));
        }

        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentNullException(nameof(parameterName));
        }

        ArgumentNullException.ThrowIfNull(allowedHosts);
        ApiKey = apiKey;
        ParameterName = parameterName;
        KeyLoc = keyLocation;
        AllowedHostsValidator = new AllowedHostsValidator(allowedHosts);
    }
    private static ActivitySource _activitySource = new(typeof(RequestInformation).Namespace!);

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using Activity? span = _activitySource?.StartActivity(nameof(AuthenticateRequestAsync));
        if (!AllowedHostsValidator.IsUrlHostValid(request.URI))
        {
            span?.SetTag("requesty.runtime.authentication.is_url_valid", false);
            return Task.CompletedTask;
        }

        Uri uri = request.URI;
        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            span?.SetTag("requesty.runtime.authentication.is_url_valid", false);
            throw new ArgumentException("Only https is supported");
        }

        switch (KeyLoc)
        {
            case KeyLocation.QueryParameter:
                string uriString = uri.OriginalString + (uri.Query != string.Empty ? "&" : "?") + $"{ParameterName}={ApiKey}";
                request.URI = new Uri(uriString);
                break;
            case KeyLocation.Header:
                request.Headers.Add(ParameterName, ApiKey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(KeyLoc));
        }
        span?.SetTag("requesty.runtime.authentication.is_url_valid", true);
        return Task.CompletedTask;
    }

    /// <summary>
    ///   The location of the API key parameter.
    /// </summary>
    public enum KeyLocation
    {
        /// <summary>
        ///   The API key is passed as a query parameter.
        /// </summary>
        QueryParameter,

        /// <summary>
        ///   The API key is passed as a header.
        /// </summary>
        Header
    }
}
