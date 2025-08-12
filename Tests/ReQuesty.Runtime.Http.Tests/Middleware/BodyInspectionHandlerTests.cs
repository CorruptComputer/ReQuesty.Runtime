using System.Text.Json;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using ReQuesty.Runtime.Http.Tests.Mocks;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware;

public class BodyInspectionHandlerTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    [Fact]
    public void BodyInspectionHandlerConstruction()
    {
        using BodyInspectionHandler defaultValue = new();
        Assert.NotNull(defaultValue);
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsRequestBodyStream()
    {
        BodyInspectionHandlerOption option = new() { InspectRequestBody = true, };
        using HttpMessageInvoker invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        HttpRequestMessage request = new(HttpMethod.Post, "https://localhost")
        {
            Content = new StringContent("request test")
        };
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync()); // response from option is separate from "normal" request stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsRequestBodyStreamWhenRequestIsOctetStream()
    {
        BodyInspectionHandlerOption option = new() { InspectRequestBody = true, };
        using HttpMessageInvoker invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        MemoryStream memoryStream = new();
        StreamWriter writer = new(memoryStream);
        await writer.WriteAsync("request test");
        await writer.FlushAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        HttpRequestMessage request = new(HttpMethod.Post, "https://localhost")
        {
            Content = new StreamContent(memoryStream)
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/octet-stream"
        );

        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync()); // response from option is separate from "normal" request stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullRequestBodyStreamWhenThereIsNoRequestBody()
    {
        BodyInspectionHandlerOption option = new() { InspectRequestBody = true, };
        using HttpMessageInvoker invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost");
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Same(Stream.Null, option.RequestBody);
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsResponseBodyStream()
    {
        BodyInspectionHandlerOption option = new() { InspectResponseBody = true, };
        using HttpMessageInvoker invoker = GetMessageInvoker(CreateHttpResponseWithBody(), option);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost");
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("response test", GetStringFromStream(option.ResponseBody!));
        Assert.Equal("response test", await response.Content.ReadAsStringAsync()); // response from option is separate from "normal" response stream
    }

    [Fact(Skip = "Test can potentially be flaky due to usage limitations on Github. Enable to verify.")]
    public async Task BodyInspectionHandlerGetsResponseBodyStreamFromGithub()
    {
        BodyInspectionHandlerOption option = new() { InspectResponseBody = true, InspectRequestBody = true };
        HttpClient httpClient = ReQuestyClientFactory.Create(optionsForHandlers: [option]);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/repos/CorruptComputer/ReQuesty.Runtime");
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        if (response.IsSuccessStatusCode)
        {
            Assert.NotEqual(Stream.Null, option.ResponseBody);
            JsonDocument jsonFromInspection = await JsonDocument.ParseAsync(option.ResponseBody);
            JsonDocument jsonFromContent = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            Assert.True(jsonFromInspection.RootElement.TryGetProperty("owner", out _));
            Assert.True(jsonFromContent.RootElement.TryGetProperty("owner", out _));
        }
        else if ((int)response.StatusCode is 429 or 403)
        {
            // We've been throttled according to the docs below. No need to fail for now.
            // https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api?apiVersion=2022-11-28#primary-rate-limit-for-unauthenticated-users
            Assert.Fail("Request was throttled");
        }
        else
        {
            Assert.Fail("Unexpected response status code in BodyInspectionHandler test");
        }
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullResponseBodyStreamWhenThereIsNoResponseBody()
    {
        BodyInspectionHandlerOption option = new() { InspectResponseBody = true, };
        using HttpMessageInvoker invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost");
        HttpResponseMessage response = await invoker.SendAsync(request, default);

        // Then
        Assert.Same(Stream.Null, option.ResponseBody);
    }

    private static HttpResponseMessage CreateHttpResponseWithBody() =>
        new() { Content = new StringContent("response test") };

    private HttpMessageInvoker GetMessageInvoker(
        HttpResponseMessage httpResponseMessage,
        BodyInspectionHandlerOption option
    )
    {
        MockRedirectHandler messageHandler = new();
        _disposables.Add(messageHandler);
        _disposables.Add(httpResponseMessage);
        messageHandler.SetHttpResponse(httpResponseMessage);
        // Given
        BodyInspectionHandler handler = new(option) { InnerHandler = messageHandler };
        _disposables.Add(handler);
        return new HttpMessageInvoker(handler);
    }

    private static string GetStringFromStream(Stream stream)
    {
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        _disposables.ForEach(static x => x.Dispose());
        GC.SuppressFinalize(this);
    }
}
