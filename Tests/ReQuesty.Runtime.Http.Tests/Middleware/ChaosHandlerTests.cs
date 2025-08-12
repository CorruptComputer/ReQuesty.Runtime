using System.Net;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware
{
    public class ChaosHandlerTests
    {
        [Fact]
        public async Task RandomChaosShouldReturnRandomFailures()
        {
            // Arrange
            ChaosHandler handler = new()
            {
                InnerHandler = new FakeSuccessHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            HttpRequestMessage request = new();

            // Act
            Dictionary<HttpStatusCode, object?> responses = [];

            // Make calls until all known failures have been triggered
            while (responses.Count < 3)
            {
                HttpResponseMessage response = await invoker.SendAsync(request, new CancellationToken());
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    responses[response.StatusCode] = null;
                }
            }

            // Assert
            Assert.True(responses.ContainsKey((HttpStatusCode)429));
            Assert.True(responses.ContainsKey(HttpStatusCode.ServiceUnavailable));
            Assert.True(responses.ContainsKey(HttpStatusCode.GatewayTimeout));
        }

        [Fact]
        public async Task RandomChaosWithCustomKnownFailuresShouldReturnAllFailuresRandomly()
        {

            // Arrange
            ChaosHandler handler = new(new ChaosHandlerOption
            {
                KnownChaos =
                [
                    ChaosHandler.Create429TooManyRequestsResponse(new TimeSpan(0,0,5)),
                    ChaosHandler.Create500InternalServerErrorResponse(),
                    ChaosHandler.Create503Response(new TimeSpan(0,0,5)),
                    ChaosHandler.Create502BadGatewayResponse(),
                    ChaosHandler.Create504GatewayTimeoutResponse(new TimeSpan(0,0,5))
                ]
            })
            {
                InnerHandler = new FakeSuccessHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            HttpRequestMessage request = new();

            // Act
            Dictionary<HttpStatusCode, object?> responses = [];

            // Make calls until all known failures have been triggered
            while (responses.Count < 5)
            {
                HttpResponseMessage response = await invoker.SendAsync(request, new CancellationToken());
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    responses[response.StatusCode] = null;
                }
            }

            // Assert
            Assert.True(responses.ContainsKey((HttpStatusCode)429));
            Assert.True(responses.ContainsKey(HttpStatusCode.InternalServerError));
            Assert.True(responses.ContainsKey(HttpStatusCode.BadGateway));
            Assert.True(responses.ContainsKey(HttpStatusCode.ServiceUnavailable));
            Assert.True(responses.ContainsKey(HttpStatusCode.GatewayTimeout));
        }

        [Fact(Skip = "Test is flaky and needs investigation.")]
        public async Task PlannedChaosShouldReturnChaosWhenPlanned()
        {
            // Arrange

            Func<HttpRequestMessage, HttpResponseMessage> plannedChaos = (req) =>
            {
                if (req.RequestUri?.OriginalString.Contains("/fail") ?? false)
                {
                    return ChaosHandler.Create429TooManyRequestsResponse(new TimeSpan(0, 0, 5));
                }
                return null!;
            };

            ChaosHandler handler = new(new ChaosHandlerOption
            {
                PlannedChaosFactory = plannedChaos
            })
            {
                InnerHandler = new FakeSuccessHandler()
            };

            // Act
            HttpRequestMessage request1 = new()
            {
                RequestUri = new Uri("http://example.org/success")
            };
            HttpResponseMessage response1 = await new HttpMessageInvoker(handler).SendAsync(request1, new CancellationToken());

            HttpRequestMessage request2 = new()
            {
                RequestUri = new Uri("http://example.org/fail")
            };
            HttpResponseMessage response2 = await new HttpMessageInvoker(handler).SendAsync(request2, new CancellationToken());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal((HttpStatusCode)429, response2.StatusCode);
        }

    }

    internal class FakeSuccessHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new()
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = request
            };
            return Task.FromResult(response);
        }
    }
}
