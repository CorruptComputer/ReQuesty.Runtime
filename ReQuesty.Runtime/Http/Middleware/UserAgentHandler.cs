using System.Diagnostics;
using System.Net.Http.Headers;
using ReQuesty.Runtime.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware
{
    /// <summary>
    /// <see cref="UserAgentHandler"/> appends the current library version to the user agent header.
    /// </summary>
    /// <param name="userAgentHandlerOption">The <see cref="UserAgentHandlerOption"/> instance to configure the user agent extension</param>
    public class UserAgentHandler(UserAgentHandlerOption? userAgentHandlerOption = null) : DelegatingHandler
    {
        private readonly UserAgentHandlerOption _userAgentOption = userAgentHandlerOption ?? new UserAgentHandlerOption();

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            Activity? activity;
            if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(UserAgentHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("requesty.runtime.handler.useragent.enable", true);
            }
            else
            {
                activity = null;
            }

            try
            {
                UserAgentHandlerOption userAgentHandlerOption = request.GetRequestOption<UserAgentHandlerOption>() ?? _userAgentOption;

                bool isProductNamePresent = false;
                foreach (ProductInfoHeaderValue userAgent in request.Headers.UserAgent)
                {
                    if (userAgentHandlerOption.ProductName.Equals(userAgent.Product?.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        isProductNamePresent = true;
                        break;
                    }
                }

                if (userAgentHandlerOption.Enabled && !isProductNamePresent)
                {
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(userAgentHandlerOption.ProductName, userAgentHandlerOption.ProductVersion));
                }
                return base.SendAsync(request, cancellationToken);
            }
            finally
            {
                activity?.Dispose();
            }
        }
    }
}
