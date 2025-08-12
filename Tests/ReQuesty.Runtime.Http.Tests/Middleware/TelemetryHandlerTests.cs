using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware
{
    public class TelemetryHandlerTests
    {
        private readonly HttpMessageInvoker _invoker;

        private readonly HttpClientRequestAdapter requestAdapter;
        public TelemetryHandlerTests()
        {
            TelemetryHandler telemetryHandler = new()
            {
                InnerHandler = new FakeSuccessHandler()
            };
            this._invoker = new HttpMessageInvoker(telemetryHandler);
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }

        [Fact]
        public async Task DefaultTelemetryHandlerDoesNotChangeRequest()
        {
            // Arrange
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            // Act and get a request message
            HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

            Assert.NotNull(requestMessage);
            Assert.Empty(requestMessage.Headers);

            // Act
            HttpResponseMessage response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert the request stays the same
            Assert.Empty(response.RequestMessage?.Headers!);
            Assert.Equal(requestMessage, response.RequestMessage);
        }

        [Fact]
        public async Task TelemetryHandlerSelectivelyEnrichesRequestsBasedOnRequestMiddleWare()
        {
            // Arrange
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            TelemetryHandlerOption telemetryHandlerOption = new()
            {
                TelemetryConfigurator = (httpRequestMessage) =>
                {
                    httpRequestMessage.Headers.Add("SdkVersion", "x.x.x");
                    return httpRequestMessage;
                }
            };
            // Configures the telemetry at the request level
            requestInfo.AddRequestOptions(new IRequestOption[] { telemetryHandlerOption });
            // Act and get a request message
            HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

            Assert.NotNull(requestMessage);
            Assert.Empty(requestMessage.Headers);

            // Act
            HttpResponseMessage response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert the request was enriched as expected
            Assert.NotEmpty(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Equal("SdkVersion", response.RequestMessage?.Headers.First().Key);
            Assert.Equal(requestMessage, response.RequestMessage);
        }

        [Fact]
        public async Task TelemetryHandlerGloballyEnrichesRequests()
        {
            // Arrange
            // Configures the telemetry at the handler level
            TelemetryHandlerOption telemetryHandlerOption = new()
            {
                TelemetryConfigurator = (httpRequestMessage) =>
                {
                    httpRequestMessage.Headers.Add("SdkVersion", "x.x.x");
                    return httpRequestMessage;
                }
            };
            TelemetryHandler handler = new(telemetryHandlerOption)
            {
                InnerHandler = new FakeSuccessHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };

            HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

            // get a request message
            Assert.NotNull(requestMessage);
            Assert.Empty(requestMessage.Headers);

            // Act
            HttpResponseMessage response = await invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert the request was enriched as expected
            Assert.NotEmpty(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Equal("SdkVersion", response.RequestMessage?.Headers.First().Key);
            Assert.Equal(requestMessage, response.RequestMessage);
        }
    }
}
