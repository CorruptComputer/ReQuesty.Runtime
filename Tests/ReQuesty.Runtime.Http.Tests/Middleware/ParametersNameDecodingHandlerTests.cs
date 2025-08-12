using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware;

public class ParametersDecodingHandlerTests
{
    private readonly HttpMessageInvoker _invoker;
    private readonly ParametersNameDecodingHandler decodingHandler;
    private readonly HttpClientRequestAdapter requestAdapter;
    public ParametersDecodingHandlerTests()
    {
        decodingHandler = new ParametersNameDecodingHandler
        {
            InnerHandler = new FakeSuccessHandler()
        };
        this._invoker = new HttpMessageInvoker(decodingHandler);
        requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
    }
    [InlineData("http://localhost?%24select=diplayName&api%2Dversion=2", "http://localhost/?$select=diplayName&api-version=2")]
    [InlineData("http://localhost?%24select=diplayName&api%7Eversion=2", "http://localhost/?$select=diplayName&api~version=2")]
    [InlineData("http://localhost?%24select=diplayName&api%2Eversion=2", "http://localhost/?$select=diplayName&api.version=2")]
    [InlineData("http://localhost/path%2dsegment?%24select=diplayName&api%2Dversion=2", "http://localhost/path-segment?$select=diplayName&api-version=2")] //it's URI decoding the path segment
    [InlineData("http://localhost:888?%24select=diplayName&api%2Dversion=2", "http://localhost:888/?$select=diplayName&api-version=2")]
    [InlineData("http://localhost", "http://localhost/")]
    [InlineData("https://google.com/?q=1%2b2", "https://google.com/?q=1%2b2")]
    [InlineData("https://google.com/?q=M%26A", "https://google.com/?q=M%26A")]
    [Theory]
    public async Task DefaultParameterNameDecodingHandlerDecodesNames(string original, string result)
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri(original)
        };
        // Act and get a request message
        HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(requestMessage);

        // Act
        await _invoker.SendAsync(requestMessage, new CancellationToken());

        // Assert the request stays the same
        Assert.Equal(result, requestMessage.RequestUri!.ToString());
    }

    [InlineData("http://localhost?%24select=diplayName&api%2Dversion=2", "http://localhost/?$select=diplayName&api-version=2")]
    [InlineData("http://localhost?%24select=diplayName&api%7Eversion=2", "http://localhost/?$select=diplayName&api~version=2")]
    [InlineData("http://localhost?%24select=diplayName&api%2Eversion=2", "http://localhost/?$select=diplayName&api.version=2")]
    [InlineData("http://localhost/path%2dsegment?%24select=diplayName&api%2Dversion=2", "http://localhost/path-segment?$select=diplayName&api-version=2")] //it's URI decoding the path segment
    [InlineData("http://localhost:888?%24select=diplayName&api%2Dversion=2", "http://localhost:888/?$select=diplayName&api-version=2")]
    [InlineData("http://localhost", "http://localhost/")]
    [InlineData("https://google.com/?q=1%2B2", "https://google.com/?q=1%2B2")]//Values are not decoded
    [InlineData("https://google.com/?q=M%26A", "https://google.com/?q=M%26A")]//Values are not decoded
    [InlineData("https://google.com/?q%2D1=M%26A", "https://google.com/?q-1=M%26A")]//Values are not decoded but params are
    [InlineData("https://google.com/?q%2D1&q=M%26A=M%26A", "https://google.com/?q-1&q=M%26A=M%26A")]//Values are not decoded but params are
    [Theory]
    public async Task DefaultParameterNameDecodingHandlerDecodesNamesWithMeaningFullUrlCharacters(string original, string result)
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri(original)
        };
        requestInfo.AddRequestOptions(new[]
        {
            new ParametersNameDecodingOption
            {
                ParametersToDecode =
                [
                    '$','.', '-', '~', '+','&' // Add custom options for testing purposes
                ]
            }
        });
        // Act and get a request message
        HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(requestMessage);

        // Act
        await _invoker.SendAsync(requestMessage, new CancellationToken());

        // Assert the request stays the same
        Assert.Equal(result, requestMessage.RequestUri!.ToString());
    }
}
