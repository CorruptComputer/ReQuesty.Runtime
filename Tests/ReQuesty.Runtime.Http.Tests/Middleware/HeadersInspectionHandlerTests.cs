using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using ReQuesty.Runtime.Http.Tests.Mocks;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware;
public class HeadersInspectionHandlerTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];
    [Fact]
    public void HeadersInspectionHandlerConstruction()
    {
        using HeadersInspectionHandler defaultValue = new();
        Assert.NotNull(defaultValue);
    }

    [Fact]
    public async Task HeadersInspectionHandlerGetsRequestHeaders()
    {
        HeadersInspectionHandlerOption option = new()
        {
            InspectRequestHeaders = true,
        };
        using HttpMessageInvoker invoker = GetMessageInvoker(option);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost");
        request.Headers.Add("test", "test");
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("test", option.RequestHeaders["test"].First());
        Assert.Empty(option.ResponseHeaders);
    }
    [Fact]
    public async Task HeadersInspectionHandlerGetsResponseHeaders()
    {
        HeadersInspectionHandlerOption option = new()
        {
            InspectResponseHeaders = true,
        };
        using HttpMessageInvoker invoker = GetMessageInvoker(option);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost");
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("test", option.ResponseHeaders["test"].First());
        Assert.Empty(option.RequestHeaders);
    }
    private HttpMessageInvoker GetMessageInvoker(HeadersInspectionHandlerOption? option = null)
    {
        MockRedirectHandler messageHandler = new();
        _disposables.Add(messageHandler);
        HttpResponseMessage response = new();
        response.Headers.Add("test", "test");
        _disposables.Add(response);
        messageHandler.SetHttpResponse(response);
        // Given
        HeadersInspectionHandler handler = new(option)
        {
            InnerHandler = messageHandler
        };
        _disposables.Add(handler);
        return new HttpMessageInvoker(handler);
    }

    public void Dispose()
    {
        _disposables.ForEach(static x => x.Dispose());
        GC.SuppressFinalize(this);
    }
}
