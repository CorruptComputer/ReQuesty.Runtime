using System.Net;
using System.Net.Http.Headers;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using ReQuesty.Runtime.Tests.Http.Mocks;

namespace ReQuesty.Runtime.Tests.Http.Middleware;

public class RedirectHandlerTests : IDisposable
{
    private readonly MockRedirectHandler _testHttpMessageHandler;
    private readonly RedirectHandler _redirectHandler;
    private readonly HttpMessageInvoker _invoker;

    public RedirectHandlerTests()
    {
        this._testHttpMessageHandler = new MockRedirectHandler();
        this._redirectHandler = new RedirectHandler
        {
            InnerHandler = _testHttpMessageHandler
        };
        this._invoker = new HttpMessageInvoker(this._redirectHandler);
    }

    public void Dispose()
    {
        this._invoker.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void RedirectHandler_Constructor()
    {
        // Assert
        using RedirectHandler redirect = new();
        Assert.Null(redirect.InnerHandler);
        Assert.NotNull(redirect.RedirectOption);
        Assert.Equal(5, redirect.RedirectOption.MaxRedirect); // default MaxRedirects is 5
        Assert.IsType<RedirectHandler>(redirect);
    }

    [Fact]
    public void RedirectHandler_HttpMessageHandlerConstructor()
    {
        // Assert
        Assert.NotNull(this._redirectHandler.InnerHandler);
        Assert.NotNull(_redirectHandler.RedirectOption);
        Assert.Equal(5, _redirectHandler.RedirectOption.MaxRedirect); // default MaxRedirects is 5
        Assert.Equal(this._redirectHandler.InnerHandler, this._testHttpMessageHandler);
        Assert.IsType<RedirectHandler>(this._redirectHandler);
    }

    [Fact]
    public void RedirectHandler_RedirectOptionConstructor()
    {
        // Assert
        using RedirectHandler redirect = new(new RedirectHandlerOption { MaxRedirect = 2, AllowRedirectOnSchemeChange = true });
        Assert.Null(redirect.InnerHandler);
        Assert.NotNull(redirect.RedirectOption);
        Assert.Equal(2, redirect.RedirectOption.MaxRedirect);
        Assert.True(redirect.RedirectOption.AllowRedirectOnSchemeChange);
        Assert.IsType<RedirectHandler>(redirect);
    }

    [Fact]
    public async Task OkStatusShouldPassThrough()
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://example.org/foo");
        HttpResponseMessage redirectResponse = new(HttpStatusCode.OK);
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse); // sets the mock response
        // Act
        HttpResponseMessage response = await this._invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Same(response.RequestMessage, httpRequestMessage);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]  // 301
    [InlineData(HttpStatusCode.Found)]  // 302
    [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
    [InlineData((HttpStatusCode)308)] // 308 not available in netstandard
    public async Task ShouldRedirectSameMethodAndContent(HttpStatusCode statusCode)
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
        {
            Content = new StringContent("Hello World")
        };
        HttpResponseMessage redirectResponse = new(statusCode);
        redirectResponse.Headers.Location = new Uri("http://example.org/bar");
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.Equal(response.RequestMessage?.Method, httpRequestMessage.Method);
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.NotNull(response.RequestMessage?.Content);
        Assert.Equal("Hello World", await response.RequestMessage.Content.ReadAsStringAsync());

    }

    [Fact]
    public async Task ShouldRedirectChangeMethodAndContent()
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
        {
            Content = new StringContent("Hello World")
        };
        HttpResponseMessage redirectResponse = new(HttpStatusCode.SeeOther);
        redirectResponse.Headers.Location = new Uri("http://example.org/bar");
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.NotEqual(response.RequestMessage?.Method, httpRequestMessage.Method);
        Assert.Equal(response.RequestMessage?.Method, HttpMethod.Get);
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.Null(response.RequestMessage?.Content);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]  // 301
    [InlineData(HttpStatusCode.Found)]  // 302
    [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
    [InlineData((HttpStatusCode)308)] // 308
    public async Task RedirectWithDifferentHostShouldRemoveAuthHeader(HttpStatusCode statusCode)
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://example.org/foo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
        HttpResponseMessage redirectResponse = new(statusCode);
        redirectResponse.Headers.Location = new Uri("http://example.net/bar");
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.NotSame(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
        Assert.Null(response.RequestMessage?.Headers.Authorization);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]  // 301
    [InlineData(HttpStatusCode.Found)]  // 302
    [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
    [InlineData((HttpStatusCode)308)] // 308
    public async Task RedirectWithDifferentSchemeThrowsInvalidOperationExceptionIfAllowRedirectOnSchemeChangeIsDisabled(HttpStatusCode statusCode)
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "https://example.org/foo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
        HttpResponseMessage redirectResponse = new(statusCode);
        redirectResponse.Headers.Location = new Uri("http://example.org/bar");
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this._invoker.SendAsync(httpRequestMessage, CancellationToken.None));
        // Assert
        Assert.Contains("Redirects with changing schemes not allowed by default", exception.Message);
        Assert.Equal("Scheme changed from https to http.", exception.InnerException?.Message);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]  // 301
    [InlineData(HttpStatusCode.Found)]  // 302
    [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
    [InlineData((HttpStatusCode)308)] // 308
    public async Task RedirectWithDifferentSchemeShouldRemoveAuthHeaderIfAllowRedirectOnSchemeChangeIsEnabled(HttpStatusCode statusCode)
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "https://example.org/foo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
        HttpResponseMessage redirectResponse = new(statusCode);
        redirectResponse.Headers.Location = new Uri("http://example.org/bar");
        this._redirectHandler.RedirectOption.AllowRedirectOnSchemeChange = true;// Enable redirects on scheme change
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.NotSame(response.RequestMessage?.RequestUri?.Scheme, httpRequestMessage.RequestUri?.Scheme);
        Assert.Null(response.RequestMessage?.Headers.Authorization);
    }

    [Fact]
    public async Task RedirectWithSameHostShouldKeepAuthHeader()
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
        HttpResponseMessage redirectResponse = new(HttpStatusCode.Redirect);
        redirectResponse.Headers.Location = new Uri("http://example.org/bar");
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.Equal(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
        Assert.NotNull(response.RequestMessage?.Headers.Authorization);
    }

    [Fact]
    public async Task RedirectWithRelativeUrlShouldKeepRequestHost()
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo");
        HttpResponseMessage redirectResponse = new(HttpStatusCode.Redirect);
        redirectResponse.Headers.Location = new Uri("/bar", UriKind.Relative);
        this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
        // Act
        HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
        // Assert
        Assert.NotSame(response.RequestMessage, httpRequestMessage);
        Assert.Equal("http://example.org/bar", response.RequestMessage?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ExceedMaxRedirectsShouldThrowsException()
    {
        // Arrange
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo");
        HttpResponseMessage response1 = new(HttpStatusCode.Redirect);
        response1.Headers.Location = new Uri("http://example.org/bar");
        HttpResponseMessage response2 = new(HttpStatusCode.Redirect);
        response2.Headers.Location = new Uri("http://example.org/foo");
        this._testHttpMessageHandler.SetHttpResponse(response1, response2);// sets the mock response
        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this._invoker.SendAsync(
               httpRequestMessage, CancellationToken.None));
        // Assert
        Assert.Equal("Too many redirects performed", exception.Message);
        Assert.Equal("Max redirects exceeded. Redirect count : 5", exception.InnerException?.Message);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
