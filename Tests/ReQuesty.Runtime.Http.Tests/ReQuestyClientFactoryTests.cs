using System.Net;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using ReQuesty.Runtime.Http.Tests.Mocks;
using Moq;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests;

public class ReQuestyClientFactoryTests
{
    [Fact]
    public void ChainHandlersCollectionAndGetFirstLinkReturnsNullOnDefaultParams()
    {
        // Act
        DelegatingHandler? delegatingHandler = ReQuestyClientFactory.ChainHandlersCollectionAndGetFirstLink();
        // Assert
        Assert.Null(delegatingHandler);
    }

    [Fact]
    public void ChainHandlersCollectionAndGetFirstLinkWithSingleHandler()
    {
        // Arrange
        TestHttpMessageHandler handler = new();
        // Act
        DelegatingHandler? delegatingHandler = ReQuestyClientFactory.ChainHandlersCollectionAndGetFirstLink(handler);
        // Assert
        Assert.NotNull(delegatingHandler);
        Assert.Null(delegatingHandler.InnerHandler);
    }

    [Fact]
    public void ChainHandlersCollectionAndGetFirstLinkWithMultipleHandlers()
    {
        // Arrange
        TestHttpMessageHandler handler1 = new();
        TestHttpMessageHandler handler2 = new();
        // Act
        DelegatingHandler? delegatingHandler = ReQuestyClientFactory.ChainHandlersCollectionAndGetFirstLink(handler1, handler2);
        // Assert
        Assert.NotNull(delegatingHandler);
        Assert.NotNull(delegatingHandler.InnerHandler); // first handler has an inner handler

        DelegatingHandler? innerHandler = delegatingHandler.InnerHandler as DelegatingHandler;
        Assert.NotNull(innerHandler);
        Assert.Null(innerHandler.InnerHandler);// end of the chain
    }

    [Fact]
    public void ChainHandlersCollectionAndGetFirstLinkWithMultipleHandlersSetsFinalHandler()
    {
        // Arrange
        TestHttpMessageHandler handler1 = new();
        TestHttpMessageHandler handler2 = new();
        HttpClientHandler finalHandler = new();
        // Act
        DelegatingHandler? delegatingHandler = ReQuestyClientFactory.ChainHandlersCollectionAndGetFirstLink(finalHandler, handler1, handler2);
        // Assert
        Assert.NotNull(delegatingHandler);
        Assert.NotNull(delegatingHandler.InnerHandler); // first handler has an inner handler

        DelegatingHandler? innerHandler = delegatingHandler.InnerHandler as DelegatingHandler;
        Assert.NotNull(innerHandler);
        Assert.NotNull(innerHandler.InnerHandler);
        Assert.IsType<HttpClientHandler>(innerHandler.InnerHandler);
    }

    [Fact]
    public void GetDefaultHttpMessageHandlerEnablesMultipleHttp2Connections()
    {
        // Act
        HttpMessageHandler defaultHandler = ReQuestyClientFactory.GetDefaultHttpMessageHandler();
        // Assert
        Assert.NotNull(defaultHandler);

        Assert.IsType<SocketsHttpHandler>(defaultHandler);
        Assert.True(((SocketsHttpHandler)defaultHandler).EnableMultipleHttp2Connections);
    }

    [Fact]
    public void GetDefaultHttpMessageHandlerSetsUpProxy()
    {
        // Arrange
        WebProxy proxy = new("http://localhost:8888", false);
        // Act
        HttpMessageHandler defaultHandler = ReQuestyClientFactory.GetDefaultHttpMessageHandler(proxy);
        // Assert
        Assert.NotNull(defaultHandler);

        Assert.IsType<SocketsHttpHandler>(defaultHandler);
        Assert.Equal(proxy, ((SocketsHttpHandler)defaultHandler).Proxy);
    }

    [Fact]
    public void CreateDefaultHandlersWithOptions()
    {
        // Arrange
        RetryHandlerOption retryHandlerOption = new() { MaxRetry = 5, ShouldRetry = (_, _, _) => true };


        // Act
        IList<DelegatingHandler> handlers = ReQuestyClientFactory.CreateDefaultHandlers([retryHandlerOption]);
        RetryHandler? retryHandler = handlers.OfType<RetryHandler>().FirstOrDefault();

        // Assert
        Assert.NotNull(retryHandler);
        Assert.Equal(retryHandlerOption, retryHandler.RetryOption);
    }

    [Fact]
    public void CreateWithNullOrEmptyHandlersReturnsHttpClient()
    {
        HttpClient client = ReQuestyClientFactory.Create(null!, (HttpMessageHandler?)null);
        Assert.IsType<HttpClient>(client);

        client = ReQuestyClientFactory.Create([]);
        Assert.IsType<HttpClient>(client);
    }

    [Fact]
    public void CreateWithAuthenticationProvider()
    {
        HttpClient client = ReQuestyClientFactory.Create(new BaseBearerTokenAuthenticationProvider(new Mock<IAccessTokenProvider>().Object));
        Assert.IsType<HttpClient>(client);
    }
}
