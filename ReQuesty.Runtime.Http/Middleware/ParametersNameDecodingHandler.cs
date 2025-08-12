using System.Diagnostics;
using ReQuesty.Runtime.Http.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http.Middleware;

/// <summary>
///   This handlers decodes special characters in the request query parameters that had to be encoded due to RFC 6570 restrictions names before executing the request.
/// </summary>
/// <param name="options">An OPTIONAL <see cref="ParametersNameDecodingOption"/> to configure <see cref="ParametersNameDecodingHandler"/></param>
public class ParametersNameDecodingHandler(ParametersNameDecodingOption? options = default) : DelegatingHandler
{
    /// <summary>
    ///   The options to use when decoding parameters names in URLs
    /// </summary>
    internal ParametersNameDecodingOption EncodingOptions
    {
        get; set;
    } = options ?? new();

    ///<inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ParametersNameDecodingOption options = request.GetRequestOption<ParametersNameDecodingOption>() ?? EncodingOptions;
        Activity? activity;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            ActivitySource activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource.StartActivity($"{nameof(ParametersNameDecodingHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("requesty.runtime.handler.parameters_name_decoding.enable", true);
        }
        else
        {
            activity = null;
        }
        try
        {
            if (!request.RequestUri!.Query.Contains("%") ||
                !options.Enabled ||
                options.ParametersToDecode == null || options.ParametersToDecode.Count == 0)
            {
                return base.SendAsync(request, cancellationToken);
            }

            Uri originalUri = request.RequestUri;
            string? query = DecodeUriEncodedString(originalUri.Query, options.ParametersToDecode.ToArray());
            Uri decodedUri = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, originalUri.AbsolutePath, query).Uri;
            request.RequestUri = decodedUri;
            return base.SendAsync(request, cancellationToken);
        }
        finally
        {
            activity?.Dispose();
        }
    }

    private static readonly char[] EntriesSeparator = ['&'];
    private static readonly char[] ParameterSeparator = ['='];

    internal static string? DecodeUriEncodedString(string? original, char[] charactersToDecode)
    {
        // for some reason static analysis is not picking up the fact that string.IsNullOrEmpty is already checking for null
        if (original is null || original.Length == 0 || charactersToDecode == null || charactersToDecode.Length == 0)
        {
            return original;
        }

        List<(string, string)> symbolsToReplace = [];
        foreach (char character in charactersToDecode)
        {
            (string, string) symbol = ($"%{Convert.ToInt32(character):X}", character.ToString());
            if (original.Contains(symbol.Item1))
            {
                symbolsToReplace.Add(symbol);
            }
        }

        List<string> encodedParameterValues = [];
        string[] parts = original.TrimStart('?').Split(EntriesSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            string parameter = part.Split(ParameterSeparator, StringSplitOptions.RemoveEmptyEntries)[0];
            if (parameter.Contains("%")) // only pull out params with `%` (encoded)
            {
                encodedParameterValues.Add(parameter);
            }
        }

        foreach (string parameter in encodedParameterValues)
        {
            string updatedParameterName = parameter;
            foreach ((string, string) symbolToReplace in symbolsToReplace)
            {
                if (parameter.Contains(symbolToReplace.Item1))
                {
                    updatedParameterName = updatedParameterName.Replace(symbolToReplace.Item1, symbolToReplace.Item2);
                }
            }
            original = original.Replace(parameter, updatedParameterName);
        }

        return original;
    }
}
