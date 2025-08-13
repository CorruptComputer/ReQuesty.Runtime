using System.Net;
using System.Runtime.Serialization;
using System.Text;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Abstractions.Store;
using Moq;
using Moq.Protected;
using ReQuesty.Runtime.Http;
using ReQuesty.Runtime.Tests.Http.Mocks;

namespace ReQuesty.Runtime.Tests.Http;

public class HttpClientRequestAdapterTests
{
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly HttpClientRequestAdapter _requestAdapter;

    public HttpClientRequestAdapterTests()
    {
        _authenticationProvider = new Mock<IAuthenticationProvider>().Object;
        _requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
    }

    [Fact]
    public void ThrowsArgumentNullExceptionOnNullAuthenticationProvider()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new HttpClientRequestAdapter(null!));
        Assert.Equal("authenticationProvider", exception.ParamName);
    }

    [Fact]
    public void BaseUrlIsSetAsExpected()
    {
        HttpClientRequestAdapter httpClientRequestAdapter = new(_authenticationProvider);
        Assert.Null(httpClientRequestAdapter.BaseUrl);// url is null

        httpClientRequestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        Assert.Equal("https://graph.microsoft.com/v1.0", httpClientRequestAdapter.BaseUrl);// url is set as expected

        httpClientRequestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0/";
        Assert.Equal("https://graph.microsoft.com/v1.0", httpClientRequestAdapter.BaseUrl);// url is does not have the last `/` character
    }

    [Fact]
    public void BaseUrlIsSetFromHttpClient()
    {
        HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        HttpClientRequestAdapter httpClientRequestAdapter = new(_authenticationProvider, httpClient: httpClient);

        Assert.NotNull(httpClientRequestAdapter.BaseUrl);// url is not null
        Assert.Equal("https://graph.microsoft.com/v1.0", httpClientRequestAdapter.BaseUrl);// url is does not have the last `/` character
    }

    [Fact]
    public void EnablesBackingStore()
    {
        // Arrange
        HttpClientRequestAdapter requestAdapter = new(_authenticationProvider);
        IBackingStoreFactory backingStore = new Mock<IBackingStoreFactory>().Object;

        //Assert the that we originally have an in memory backing store
        Assert.IsAssignableFrom<InMemoryBackingStoreFactory>(BackingStoreFactorySingleton.Instance);

        // Act
        requestAdapter.EnableBackingStore(backingStore);

        //Assert the backing store has been updated
        Assert.IsAssignableFrom(backingStore.GetType(), BackingStoreFactorySingleton.Instance);
    }


    [Fact]
    public async Task GetRequestMessageFromRequestInformationWithBaseUrlTemplate()
    {
        // Arrange
        _requestAdapter.BaseUrl = "http://localhost";
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/me"
        };

        // Act
        HttpRequestMessage? requestMessage = await _requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

        // Assert
        Assert.NotNull(requestMessage);
        Assert.NotNull(requestMessage.RequestUri);
        Assert.Contains("http://localhost/me", requestMessage.RequestUri.OriginalString);
    }

    [Fact]
    public async Task GetRequestMessageFromRequestInformationUsesBaseUrlFromAdapter()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/me",
            PathParameters = new Dictionary<string, object>
            {
                { "baseurl", "https://graph.microsoft.com/beta"}//request information with different base url
            }

        };
        // Change the baseUrl of the adapter
        _requestAdapter.BaseUrl = "http://localhost";

        // Act
        HttpRequestMessage? requestMessage = await _requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

        // Assert
        Assert.NotNull(requestMessage);
        Assert.NotNull(requestMessage.RequestUri);
        Assert.Contains("http://localhost/me", requestMessage.RequestUri.OriginalString);// Request generated using adapter baseUrl
    }

    [Theory]
    [InlineData("select", new[] { "id", "displayName" }, "select=id,displayName")]
    [InlineData("count", true, "count=true")]
    [InlineData("skip", 10, "skip=10")]
    [InlineData("skip", null, "")]// query parameter no placed
    public async Task GetRequestMessageFromRequestInformationSetsQueryParametersCorrectlyWithSelect(string queryParam, object? queryParamObject, string expectedString)
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?top,skip,search,filter,count,orderby,select}"
        };
        requestInfo.QueryParameters.Add(queryParam, queryParamObject!);

        // Act
        HttpRequestMessage? requestMessage = await _requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

        // Assert
        Assert.NotNull(requestMessage);
        Assert.NotNull(requestMessage.RequestUri);
        Assert.Contains(expectedString, requestMessage.RequestUri.Query);
    }

    [Fact]
    public async Task GetRequestMessageFromRequestInformationSetsContentHeaders()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.PUT,
            UrlTemplate = "https://sn3302.up.1drv.com/up/fe6987415ace7X4e1eF866337"
        };
        requestInfo.Headers.Add("Content-Length", "26");
        requestInfo.Headers.Add("Content-Range", "bytes 0-25/128");
        requestInfo.SetStreamContent(new MemoryStream(Encoding.UTF8.GetBytes("contents")), "application/octet-stream");

        // Act
        HttpRequestMessage? requestMessage = await _requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

        // Assert
        Assert.NotNull(requestMessage);
        Assert.NotNull(requestMessage.Content);
        // Content length set correctly
        Assert.Equal(26, requestMessage.Content.Headers.ContentLength);
        // Content range set correctly
        Assert.NotNull(requestMessage.Content.Headers.ContentRange);
        Assert.Equal("bytes", requestMessage.Content.Headers.ContentRange.Unit);
        Assert.Equal(0, requestMessage.Content.Headers.ContentRange.From);
        Assert.Equal(25, requestMessage.Content.Headers.ContentRange.To);
        Assert.Equal(128, requestMessage.Content.Headers.ContentRange.Length);
        Assert.True(requestMessage.Content.Headers.ContentRange.HasRange);
        Assert.True(requestMessage.Content.Headers.ContentRange.HasLength);
        // Content type set correctly
        Assert.Equal("application/octet-stream", requestMessage.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SendMethodDoesNotThrowWithoutUrlTemplate()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Test")))
            });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

        Assert.NotNull(response);
        Assert.True(response.CanRead);
        Assert.Equal(4, response.Length);
    }

    [InlineData(HttpStatusCode.Redirect)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [Theory]
    public async Task SendMethodDoesNotThrowOn3XXWithNoLocationAsync(HttpStatusCode httpStatusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = httpStatusCode
            });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        MockEntity? response = await adapter.SendAsync(requestInfo, MockEntity.Factory);

        Assert.Null(response);
    }

    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [InlineData(HttpStatusCode.PartialContent)]
    [Theory]
    public async Task SendStreamReturnsUsableStream(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Test")))
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

        Assert.NotNull(response);
        Assert.True(response.CanRead);
        Assert.Equal(4, response.Length);
        StreamReader streamReader = new(response);
        string responseString = await streamReader.ReadToEndAsync();
        Assert.Equal("Test", responseString);
    }
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [InlineData(HttpStatusCode.NoContent)]
    [Theory]
    public async Task SendStreamReturnsNullForNoContent(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

        Assert.Null(response);
    }
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [Theory]
    public async Task SendSNoContentDoesntFailOnOtherStatusCodes(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        await adapter.SendNoContentAsync(requestInfo);
    }
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.ResetContent)]
    [Theory]
    public async Task SendReturnsNullOnNoContent(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        MockEntity? response = await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        Assert.Null(response);
    }

    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.ResetContent)]
    [Theory]
    public async Task SendReturnsNullOnNoContentWithContentHeaderPresent(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        MockEntity? response = await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        Assert.Null(response);
    }
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
    [Theory]
    public async Task SendReturnsObjectOnContent(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        using StreamContent mockContent = new(new MemoryStream(Encoding.UTF8.GetBytes("Test")));
        mockContent.Headers.ContentType = new("application/json");
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = mockContent,
        });
        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<MockEntity>>()))
        .Returns(new MockEntity());
        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client, parseNodeFactory: mockParseNodeFactory.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        MockEntity? response = await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        Assert.NotNull(response);
    }
    [Fact]
    public async Task RetriesOnCAEResponse()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        bool methodCalled = false;
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .Returns<HttpRequestMessage, CancellationToken>((mess, token) =>
        {
            HttpResponseMessage response = new()
            {
                StatusCode = methodCalled ? HttpStatusCode.OK : HttpStatusCode.Unauthorized,
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Test")))
            };
            if (!methodCalled)
            {
                response.Headers.WwwAuthenticate.Add(new("Bearer", "realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\"00000003-0000-0000-c000-000000000000\", error=\"insufficient_claims\", claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTY1MjgxMzUwOCJ9fX0=\""));
            }

            methodCalled = true;
            return Task.FromResult(response);
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };

        Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

        Assert.NotNull(response);

        mockHandler.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.BadGateway)]
    [Theory]
    public async Task SetsTheApiExceptionStatusCode(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(() =>
        {
            HttpResponseMessage responseMessage = new()
            {
                StatusCode = statusCode
            };
            responseMessage.Headers.Add("request-id", "guid-value");
            return responseMessage;
        });
        HttpClientRequestAdapter adapter = new(_authenticationProvider, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };
        try
        {
            Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);
            Assert.Fail("Expected an ApiException to be thrown");
        }
        catch (ApiException e)
        {
            Assert.Equal((int)statusCode, e.ResponseStatusCode);
            Assert.True(e.ResponseHeaders.ContainsKey("request-id"));
        }
    }
    [InlineData(HttpStatusCode.NotFound)]// 4XX
    [InlineData(HttpStatusCode.BadGateway)]// 5XX
    [Theory]
    public async Task SelectsTheXXXErrorMappingClassCorrectly(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(() =>
        {
            HttpResponseMessage responseMessage = new()
            {
                StatusCode = statusCode,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            return responseMessage;
        });
        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<IParsable>>()))
        .Returns(new MockError("A general error occured"));
        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };
        try
        {
            Dictionary<string, ParsableFactory<IParsable>> errorMapping = new()
            {
                { "XXX", (parseNode) => new MockError("A general error occured")},
            };
            Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo, errorMapping);
            Assert.Fail("Expected an ApiException to be thrown");
        }
        catch (MockError mockError)
        {
            Assert.Equal((int)statusCode, mockError.ResponseStatusCode);
            Assert.Equal("A general error occured", mockError.Message);
        }
    }
    [InlineData(HttpStatusCode.BadGateway)]// 5XX
    [Theory]
    public async Task ThrowsApiExceptionOnMissingMapping(HttpStatusCode statusCode)
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(() =>
        {
            HttpResponseMessage responseMessage = new()
            {
                StatusCode = statusCode,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            return responseMessage;
        });
        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<IParsable>>()))
        .Returns(new MockError("A general error occured: " + statusCode.ToString()));
        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "https://example.com"
        };
        try
        {
            Dictionary<string, ParsableFactory<IParsable>> errorMapping = new()
            {
                { "4XX", (parseNode) => new MockError("A 4XX error occured") }//Only 4XX
            };
            Stream? response = await adapter.SendPrimitiveAsync<Stream>(requestInfo, errorMapping);
            Assert.Fail("Expected an ApiException to be thrown");
        }
        catch (ApiException apiException)
        {
            Assert.Equal((int)statusCode, apiException.ResponseStatusCode);
            Assert.Contains("The server returned an unexpected status code and no error factory is registered for this code", apiException.Message);
        }
    }
    [Fact]
    public async Task SendPrimitiveHandleEnumIfValueIsString()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value1")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value1");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum response = await adapter.SendPrimitiveAsync<TestEnum>(requestInfo);

        Assert.Equal(TestEnum.Value1, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumIfValueIsString()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value1")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value1");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum? response = await adapter.SendPrimitiveAsync<TestEnum?>(requestInfo);

        Assert.Equal(TestEnum.Value1, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleEnumIfValueIsInteger()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("1")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("1");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum response = await adapter.SendPrimitiveAsync<TestEnum>(requestInfo);

        Assert.Equal(TestEnum.Value2, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumIfValueIsInteger()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("1")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("1");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum? response = await adapter.SendPrimitiveAsync<TestEnum?>(requestInfo);

        Assert.Equal(TestEnum.Value2, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleEnumIfValueIsFromEnumMember()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value__3")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value__3");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum response = await adapter.SendPrimitiveAsync<TestEnum>(requestInfo);

        Assert.Equal(TestEnum.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumIfValueIsFromEnumMember()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value__3")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value__3");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum? response = await adapter.SendPrimitiveAsync<TestEnum?>(requestInfo);

        Assert.Equal(TestEnum.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveReturnsNullIfValueCannotBeParsedToEnum()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value0")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value0");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnum? response = await adapter.SendPrimitiveAsync<TestEnum?>(requestInfo);

        Assert.Null(response);
    }

    [Fact]
    public async Task SendPrimitiveHandleEnumFlagsIfValuesAreStrings()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value1,Value3")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value1,Value3");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags response = await adapter.SendPrimitiveAsync<TestEnumWithFlags>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumFlagsIfValuesAreStrings()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value1,Value3")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value1,Value3");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags? response = await adapter.SendPrimitiveAsync<TestEnumWithFlags?>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleEnumFlagsIfValuesAreIntegers()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("1,2")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("1,2");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags response = await adapter.SendPrimitiveAsync<TestEnumWithFlags>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumFlagsIfValuesAreIntegers()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("1,2")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("1,2");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags? response = await adapter.SendPrimitiveAsync<TestEnumWithFlags?>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleEnumFlagsIfValuesAreFromEnumMember()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value__3,Value__2")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value__3,Value__2");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags response = await adapter.SendPrimitiveAsync<TestEnumWithFlags>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value2 | TestEnumWithFlags.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveHandleNullableEnumFlagsIfValuesAreFromEnumMember()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value__3,Value__2")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value__3,Value__2");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags? response = await adapter.SendPrimitiveAsync<TestEnumWithFlags?>(requestInfo);

        Assert.Equal(TestEnumWithFlags.Value2 | TestEnumWithFlags.Value3, response);
    }

    [Fact]
    public async Task SendPrimitiveReturnsNullIfFlagValueCannotBeParsedToEnum()
    {
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient client = new(mockHandler.Object);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Value0")
            });

        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetStringValue())
        .Returns("Value0");

        Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
        mockParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockParseNode.Object));
        HttpClientRequestAdapter adapter = new(_authenticationProvider, mockParseNodeFactory.Object, httpClient: client);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        TestEnumWithFlags? response = await adapter.SendPrimitiveAsync<TestEnumWithFlags?>(requestInfo);

        Assert.Null(response);
    }
}

public enum TestEnum
{
    [EnumMember(Value = "Value__1")]
    Value1,
    [EnumMember(Value = "Value__2")]
    Value2,
    [EnumMember(Value = "Value__3")]
    Value3
}

[Flags]
public enum TestEnumWithFlags
{
    [EnumMember(Value = "Value__1")]
    Value1 = 0x01,
    [EnumMember(Value = "Value__2")]
    Value2 = 0x02,
    [EnumMember(Value = "Value__3")]
    Value3 = 0x04
}
