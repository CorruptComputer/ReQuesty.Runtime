using System.Text;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Extensions;
using ReQuesty.Runtime.Http.Middleware.Options;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Extensions;

public class HttpRequestMessageExtensionsTests
{
    private readonly HttpClientRequestAdapter requestAdapter = new(new AnonymousAuthenticationProvider());

    [Fact]
    public async Task GetRequestOptionCanExtractRequestOptionFromHttpRequestMessage()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        RedirectHandlerOption redirectHandlerOption = new()
        {
            MaxRedirect = 7
        };
        requestInfo.AddRequestOptions(new IRequestOption[] { redirectHandlerOption });
        // Act and get a request message
        HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(requestMessage);

        RedirectHandlerOption? extractedOption = requestMessage.GetRequestOption<RedirectHandlerOption>();
        // Assert
        Assert.NotNull(extractedOption);
        Assert.Equal(redirectHandlerOption, extractedOption);
        Assert.Equal(7, redirectHandlerOption.MaxRedirect);
    }

    [Fact]
    public async Task CloneAsyncWithEmptyHttpContent()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();

        Assert.NotNull(clonedRequest);
        Assert.Equal(originalRequest.Method, clonedRequest.Method);
        Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
        Assert.Null(clonedRequest.Content);
    }

    [Fact]
    public async Task CloneAsyncWithHttpContent()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        originalRequest.Content = new StringContent("contents");

        HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();
        string originalContents = await originalRequest.Content.ReadAsStringAsync();
        string clonedRequestContents = await clonedRequest.Content!.ReadAsStringAsync();

        Assert.NotNull(clonedRequest);
        Assert.Equal(originalRequest.Method, clonedRequest.Method);
        Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
        Assert.Equal(originalContents, clonedRequestContents);
        Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
    }

    [Fact]
    public async Task CloneAsyncWithHttpStreamContent()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        requestInfo.SetStreamContent(new MemoryStream(Encoding.UTF8.GetBytes("contents")), "application/octet-stream");
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();
        string originalContents = await originalRequest.Content!.ReadAsStringAsync();
        string clonedRequestContents = await clonedRequest.Content!.ReadAsStringAsync();

        Assert.NotNull(clonedRequest);
        Assert.Equal(originalRequest.Method, clonedRequest.Method);
        Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
        Assert.Equal(originalContents, clonedRequestContents);
        Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
    }

    [Fact]
    public async Task CloneAsyncWithRequestOption()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        RedirectHandlerOption redirectHandlerOption = new()
        {
            MaxRedirect = 7
        };
        requestInfo.AddRequestOptions(new IRequestOption[] { redirectHandlerOption });
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        originalRequest.Content = new StringContent("contents");

        HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();

        Assert.NotNull(clonedRequest);
        Assert.Equal(originalRequest.Method, clonedRequest.Method);
        Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
        Assert.NotEmpty(clonedRequest.Options);
        Assert.Equal(redirectHandlerOption, clonedRequest.Options.First().Value);
        Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
    }

    [Fact]
    public async Task IsBufferedReturnsTrueForGetRequest()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        // Act
        bool response = originalRequest.IsBuffered();
        // Assert
        Assert.True(response, "Unexpected content type");
    }
    [Fact]
    public async Task IsBufferedReturnsTrueForPostWithNoContent()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            URI = new Uri("http://localhost")
        };
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        // Act
        bool response = originalRequest.IsBuffered();
        // Assert
        Assert.True(response, "Unexpected content type");
    }
    [Fact]
    public async Task IsBufferedReturnsTrueForPostWithBufferStringContent()
    {
        // Arrange
        byte[] data = new byte[] { 1, 2, 3, 4, 5 };
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            URI = new Uri("http://localhost"),
            Content = new MemoryStream(data)
        };
        HttpRequestMessage? originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        Assert.NotNull(originalRequest);

        // Act
        bool response = originalRequest.IsBuffered();
        // Assert
        Assert.True(response, "Unexpected content type");
    }
}
