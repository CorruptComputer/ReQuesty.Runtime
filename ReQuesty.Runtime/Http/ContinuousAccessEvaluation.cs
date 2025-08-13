﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ReQuesty.Runtime.Http
{
    /// <summary>
    /// Process continuous access evaluation
    /// </summary>
    static internal class ContinuousAccessEvaluation
    {
        internal const string ClaimsKey = "claims";
        internal const string BearerAuthenticationScheme = "Bearer";
        private static readonly char[] ComaSplitSeparator = [','];
        private static Func<AuthenticationHeaderValue, bool> filterAuthHeader = static x => x.Scheme.Equals(BearerAuthenticationScheme, StringComparison.OrdinalIgnoreCase);
        private static readonly Regex caeValueRegex = new("\"([^\"]*)\"", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        /// <summary>
        ///    Extracts claims header value from a response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetClaims(HttpResponseMessage response)
        {
            ArgumentNullException.ThrowIfNull(response);
            if (response.StatusCode != HttpStatusCode.Unauthorized
               || response.Headers.WwwAuthenticate.Count == 0)
            {
                return string.Empty;
            }
            AuthenticationHeaderValue? authHeader = null;
            foreach (AuthenticationHeaderValue header in response.Headers.WwwAuthenticate)
            {
                if (filterAuthHeader(header))
                {
                    authHeader = header;
                    break;
                }
            }
            if (authHeader is not null)
            {
                string[]? authHeaderParameters = authHeader.Parameter?.Split(ComaSplitSeparator, StringSplitOptions.RemoveEmptyEntries);

                string? rawResponseClaims = null;
                if (authHeaderParameters != null)
                {
                    foreach (string parameter in authHeaderParameters)
                    {
                        string trimmedParameter = parameter.Trim();
                        if (trimmedParameter.StartsWith(ClaimsKey, StringComparison.OrdinalIgnoreCase))
                        {
                            rawResponseClaims = trimmedParameter;
                            break;
                        }
                    }
                }

                if (rawResponseClaims != null &&
                   caeValueRegex.Match(rawResponseClaims) is Match claimsMatch &&
                   claimsMatch.Groups.Count > 1 &&
                   claimsMatch.Groups[1].Value is string responseClaims)
                {
                    return responseClaims;
                }

            }
            return string.Empty;
        }
    }
}

