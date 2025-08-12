using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware
{
    public class UserAgentHandlerTests
    {
        private readonly HttpMessageInvoker _invoker;
        private readonly HttpClientRequestAdapter requestAdapter;
        public UserAgentHandlerTests()
        {
            UserAgentHandler userAgentHandler = new()
            {
                InnerHandler = new FakeSuccessHandler()
            };
            this._invoker = new HttpMessageInvoker(userAgentHandler);
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }

        [Fact]
        public async Task DisabledUserAgentHandlerDoesNotChangeRequest()
        {
            // Arrange
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            requestInfo.AddRequestOptions(new[] {
                new UserAgentHandlerOption
                {
                    Enabled = false
                }
            });
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
        public async Task EnabledUserAgentHandlerAddsHeaderValue()
        {
            // Arrange
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            UserAgentHandlerOption defaultOption = new();
            // Act and get a request message
            HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

            Assert.NotNull(requestMessage);
            Assert.Empty(requestMessage.Headers);

            // Act
            HttpResponseMessage response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!.UserAgent!);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.First().Product?.Name, defaultOption.ProductName, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.First().Product?.Version, defaultOption.ProductVersion, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.ToString(), $"{defaultOption.ProductName}/{defaultOption.ProductVersion}", StringComparer.OrdinalIgnoreCase);
            Assert.Equal(requestMessage, response.RequestMessage);
        }

        [Fact]
        public async Task DoesntAddProductTwice()
        {
            // Arrange
            RequestInformation requestInfo = new()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            UserAgentHandlerOption defaultOption = new();
            // Act and get a request message
            HttpRequestMessage? requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

            Assert.NotNull(requestMessage);
            Assert.Empty(requestMessage.Headers);

            // Act
            HttpResponseMessage response = await _invoker.SendAsync(requestMessage, new CancellationToken());
            response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!.UserAgent!);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.ToString(), $"{defaultOption.ProductName}/{defaultOption.ProductVersion}", StringComparer.OrdinalIgnoreCase);
        }

    }

}
