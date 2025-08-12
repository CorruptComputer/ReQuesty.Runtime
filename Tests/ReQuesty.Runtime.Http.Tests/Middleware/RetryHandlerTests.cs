using System.Globalization;
using System.Net;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using ReQuesty.Runtime.Http.Tests.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware
{
    public class RetryHandlerTests : IDisposable
    {
        private readonly MockRedirectHandler _testHttpMessageHandler;
        private readonly RetryHandler _retryHandler;
        private readonly HttpMessageInvoker _invoker;
        private const string RetryAfter = "Retry-After";
        private const string RetryAttempt = "Retry-Attempt";

        public RetryHandlerTests()
        {
            _testHttpMessageHandler = new MockRedirectHandler();
            _retryHandler = new RetryHandler
            {
                InnerHandler = _testHttpMessageHandler
            };
            _invoker = new HttpMessageInvoker(_retryHandler);
        }

        public void Dispose()
        {
            _invoker.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void RetryHandlerConstructor()
        {
            // Act
            using RetryHandler retry = new();
            // Assert
            Assert.Null(retry.InnerHandler);
            Assert.NotNull(retry.RetryOption);
            Assert.Equal(RetryHandlerOption.DefaultMaxRetry, retry.RetryOption.MaxRetry);
            Assert.IsType<RetryHandler>(retry);
        }

        [Fact]
        public void RetryHandlerHttpMessageHandlerConstructor()
        {
            // Assert
            Assert.NotNull(_retryHandler.InnerHandler);
            Assert.NotNull(_retryHandler.RetryOption);
            Assert.Equal(RetryHandlerOption.DefaultMaxRetry, _retryHandler.RetryOption.MaxRetry);
            Assert.Equal(_retryHandler.InnerHandler, _testHttpMessageHandler);
            Assert.IsType<RetryHandler>(_retryHandler);
        }

        [Fact]
        public void RetryHandlerRetryOptionConstructor()
        {
            // Act
            using RetryHandler retry = new(new RetryHandlerOption { MaxRetry = 5, ShouldRetry = (_, _, _) => true });
            // Assert
            Assert.Null(retry.InnerHandler);
            Assert.NotNull(retry.RetryOption);
            Assert.Equal(5, retry.RetryOption.MaxRetry);
            Assert.IsType<RetryHandler>(retry);
        }

        [Fact]
        public async Task OkStatusShouldPassThrough()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://example.org/foo");
            HttpResponseMessage retryResponse = new(HttpStatusCode.OK);
            _testHttpMessageHandler.SetHttpResponse(retryResponse);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, retryResponse);
            Assert.NotNull(response.RequestMessage);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.False(response.RequestMessage.Headers.Contains(RetryAttempt), "The request add header wrong.");
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithAddRetryAttemptHeader(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://example.org/foo");
            HttpResponseMessage retryResponse = new(statusCode);
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, response2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Headers);
            Assert.True(response.RequestMessage.Headers.Contains(RetryAttempt));
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out IEnumerable<string>? values));
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithBuffedContent(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            HttpResponseMessage retryResponse = new(statusCode);
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, response2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal("Hello World", await response.RequestMessage.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPostStreaming(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;
            HttpResponseMessage retryResponse = new(statusCode);
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.NotEqual(response, response2);
            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPutStreaming(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;
            HttpResponseMessage retryResponse = new(statusCode);
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.NotEqual(response, response2);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, retryResponse);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ExceedMaxRetryShouldReturn(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo");
            HttpResponseMessage retryResponse = new(statusCode);
            HttpResponseMessage response2 = new(statusCode);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            RetryHandler retryHandler = new()
            {
                InnerHandler = _testHttpMessageHandler,
                RetryOption = new RetryHandlerOption { Delay = 1 }
            };
            HttpMessageInvoker invoker = new(retryHandler);
            // Act
            try
            {
                await invoker.SendAsync(httpRequestMessage, new CancellationToken());
            }
            catch (Exception exception)
            {
                // Assert
                Assert.IsType<AggregateException>(exception);
                AggregateException? aggregateException = exception as AggregateException;
                Assert.StartsWith("Too many retries performed.", aggregateException!.Message);
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.IsType<ApiException>(innerexception));
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.True((innerexception as ApiException)!.ResponseStatusCode == (int)statusCode));
                Assert.False(httpRequestMessage.Headers.TryGetValues(RetryAttempt, out _), "Don't set Retry-Attempt Header");
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnRetryAfterHeaderWithSeconds(HttpStatusCode statusCode)
        {
            // Arrange
            HttpResponseMessage retryResponse = new(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 1.ToString());
            // Act
            await DelayTestWithMessage(retryResponse, 1, "Init");
            // Assert
            Assert.Equal("Init Work 1", Message);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnRetryAfterHeaderWithHttpDate(HttpStatusCode statusCode)
        {
            // Arrange
            HttpResponseMessage retryResponse = new(statusCode);
            DateTime futureTime = DateTime.Now + TimeSpan.FromSeconds(3);// 3 seconds from now
            string futureTimeString = futureTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture);
            Assert.Contains("GMT", futureTimeString); // http date always end in GMT according to the spec
            Assert.True(retryResponse.Headers.TryAddWithoutValidation(RetryAfter, futureTimeString));
            // Act
            await DelayTestWithMessage(retryResponse, 1, "Init");
            // Assert
            Assert.Equal("Init Work 1", Message);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnExponentialBackOff(HttpStatusCode statusCode)
        {
            // Arrange
            HttpResponseMessage retryResponse = new(statusCode);
            string compareMessage = "Init Work ";

            for (int count = 0; count < 3; count++)
            {
                // Act
                await DelayTestWithMessage(retryResponse, count, "Init", 1);
                // Assert
                Assert.Equal(Message, compareMessage + count);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldReturnSameStatusCodeWhenDelayIsGreaterThanRetryTimeLimit(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            HttpResponseMessage retryResponse = new(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 20.ToString());
            _retryHandler.RetryOption.RetriesTimeLimit = TimeSpan.FromSeconds(10);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, retryResponse);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryBasedOnRetryAfterHeaderWithSeconds(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            HttpResponseMessage retryResponse = new(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 5.ToString());
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, response2);
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out IEnumerable<string>? values), "Don't set Retry-Attempt Header");
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryBasedOnRetryAfterHeaderWithHttpDate(HttpStatusCode statusCode)
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            HttpResponseMessage retryResponse = new(statusCode);
            DateTime futureTime = DateTime.Now + TimeSpan.FromSeconds(5);// 5 seconds from now
            string futureTimeString = futureTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, futureTimeString);
            Assert.Contains("GMT", futureTimeString); // http date always end in GMT according to the spec
            HttpResponseMessage response2 = new(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            HttpResponseMessage response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.Same(response, response2);
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out IEnumerable<string>? values), "Don't set Retry-Attempt Header");
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.BadGateway, true)]
        [InlineData(2, HttpStatusCode.BadGateway, true)]
        [InlineData(3, HttpStatusCode.BadGateway, true)]
        [InlineData(4, HttpStatusCode.OK, false)]
        public async Task ShouldRetryBasedOnCustomShouldRetryDelegate(int expectedMaxRetry, HttpStatusCode expectedStatusCode, bool isExceptionExpected)
        {
            // Arrange
            HttpRequestMessage request = new();
            Queue<HttpResponseMessage> httpResponseQueue = new(new HttpResponseMessage[]
            {
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.OK) { RequestMessage = request },
            });

            Mock<HttpMessageHandler> mockHttpMessageHandler = new(MockBehavior.Loose);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                .Returns(() =>
                    {
                        HttpResponseMessage response;
                        try
                        {
                            response = httpResponseQueue.Dequeue();
                        }
                        catch
                        {
                            response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
                        }
                        return Task.FromResult(response);
                    })
                .Verifiable();

            RetryHandler retryHandler = new(new RetryHandlerOption()
            {
                ShouldRetry = (_, _, httpResponseMessage) => httpResponseMessage.StatusCode == HttpStatusCode.BadGateway,
                MaxRetry = expectedMaxRetry,
                Delay = 0
            })
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            HttpMessageInvoker httpMessageInvoker = new(retryHandler);

            // Act
            try
            {
                HttpResponseMessage response = await httpMessageInvoker.SendAsync(request, new CancellationToken());

                Assert.False(isExceptionExpected);
                Assert.Equal(expectedStatusCode, response.StatusCode);
            }
            catch (Exception exception)
            {
                // Assert
                Assert.IsType<AggregateException>(exception);
                AggregateException? aggregateException = exception as AggregateException;
                Assert.True(isExceptionExpected);
                Assert.StartsWith("Too many retries performed.", aggregateException!.Message);
                Assert.Equal(1 + expectedMaxRetry, aggregateException.InnerExceptions.Count);
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.Contains(expectedStatusCode.ToString(), innerexception.Message));
            }

            // Assert
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(1 + expectedMaxRetry), ItExpr.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>());
        }

        private async Task DelayTestWithMessage(HttpResponseMessage response, int count, string message, int delay = RetryHandlerOption.MaxDelay)
        {
            Message = message;
            await Task.Run(async () =>
            {
                await RetryHandler.DelayAsync(response, count, delay, out _, new CancellationToken());
                Message += " Work " + count;
            });
        }

        private string? Message
        {
            get;
            set;
        }
    }
}
