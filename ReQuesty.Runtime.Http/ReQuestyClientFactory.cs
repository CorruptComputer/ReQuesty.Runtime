﻿using System.Diagnostics.CodeAnalysis;
using System.Net;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;

namespace ReQuesty.Runtime.Http;

/// <summary>
///   This class is used to build the HttpClient instance used by the core service.
/// </summary>
public static class ReQuestyClientFactory
{
    /// <summary>
    ///   Initializes the <see cref="HttpClient"/> with the default configuration and middlewares including a authentication middleware using the <see cref="IAuthenticationProvider"/> if provided.
    /// </summary>
    /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects </param>
    /// <param name="optionsForHandlers">A array of <see cref="IRequestOption"/> objects passed to the default handlers.</param>
    /// <returns>The <see cref="HttpClient"/> with the default middlewares.</returns>
    public static HttpClient Create(HttpMessageHandler? finalHandler = null, IRequestOption[]? optionsForHandlers = null)
    {
        IList<DelegatingHandler> defaultHandlersEnumerable = CreateDefaultHandlers(optionsForHandlers);
        int count = 0;
        foreach (DelegatingHandler _ in defaultHandlersEnumerable)
        {
            count++;
        }

        DelegatingHandler[] defaultHandlersArray = new DelegatingHandler[count];
        int index = 0;
        foreach (DelegatingHandler handler2 in defaultHandlersEnumerable)
        {
            defaultHandlersArray[index++] = handler2;
        }
        DelegatingHandler? handler = ChainHandlersCollectionAndGetFirstLink(finalHandler ?? GetDefaultHttpMessageHandler(), defaultHandlersArray);
        return handler != null ? new HttpClient(handler) : new HttpClient();
    }

    /// <summary>
    ///   Initializes the <see cref="HttpClient"/> with a custom middleware pipeline.
    /// </summary>
    /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
    /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects</param>
    /// <returns>The <see cref="HttpClient"/> with the custom handlers.</returns>
    public static HttpClient Create(IList<DelegatingHandler> handlers, HttpMessageHandler? finalHandler = null)
    {
        if (handlers == null || handlers.Count == 0)
        {
            return Create(finalHandler);
        }

        DelegatingHandler[] handlersArray = new DelegatingHandler[handlers.Count];
        for (int i = 0; i < handlers.Count; i++)
        {
            handlersArray[i] = handlers[i];
        }

        DelegatingHandler? handler = ChainHandlersCollectionAndGetFirstLink(finalHandler ?? GetDefaultHttpMessageHandler(), handlersArray);
        return handler != null ? new HttpClient(handler) : new HttpClient();
    }

    /// <summary>
    ///   Initializes the <see cref="HttpClient"/> with the default configuration and authentication middleware using the <see cref="IAuthenticationProvider"/> if provided.
    /// </summary>
    /// <param name="authenticationProvider"></param>
    /// <param name="optionsForHandlers"></param>
    /// <param name="finalHandler"></param>
    /// <returns></returns>
    public static HttpClient Create(BaseBearerTokenAuthenticationProvider authenticationProvider, IRequestOption[]? optionsForHandlers = null, HttpMessageHandler? finalHandler = null)
    {
        IList<DelegatingHandler> defaultHandlersEnumerable = CreateDefaultHandlers(optionsForHandlers);
        defaultHandlersEnumerable.Add(new AuthorizationHandler(authenticationProvider));
        return Create(defaultHandlersEnumerable, finalHandler);
    }

    /// <summary>
    ///   Creates a default set of middleware to be used by the <see cref="HttpClient"/>.
    /// </summary>
    /// <returns>A list of the default handlers used by the client.</returns>
    public static IList<DelegatingHandler> CreateDefaultHandlers(IRequestOption[]? optionsForHandlers = null)
    {
        optionsForHandlers ??= [];

        UriReplacementHandlerOption? uriReplacementOption = null;
        RetryHandlerOption? retryHandlerOption = null;
        RedirectHandlerOption? redirectHandlerOption = null;
        ParametersNameDecodingOption? parametersNameDecodingOption = null;
        UserAgentHandlerOption? userAgentHandlerOption = null;
        HeadersInspectionHandlerOption? headersInspectionHandlerOption = null;
        BodyInspectionHandlerOption? bodyInspectionHandlerOption = null;

        foreach (IRequestOption option in optionsForHandlers)
        {
            if (uriReplacementOption == null && option is UriReplacementHandlerOption uriOption)
            {
                uriReplacementOption = uriOption;
            }
            else if (retryHandlerOption == null && option is RetryHandlerOption retryOption)
            {
                retryHandlerOption = retryOption;
            }
            else if (redirectHandlerOption == null && option is RedirectHandlerOption redirectOption)
            {
                redirectHandlerOption = redirectOption;
            }
            else if (parametersNameDecodingOption == null && option is ParametersNameDecodingOption parametersOption)
            {
                parametersNameDecodingOption = parametersOption;
            }
            else if (userAgentHandlerOption == null && option is UserAgentHandlerOption userAgentOption)
            {
                userAgentHandlerOption = userAgentOption;
            }
            else if (headersInspectionHandlerOption == null && option is HeadersInspectionHandlerOption headersInspectionOption)
            {
                headersInspectionHandlerOption = headersInspectionOption;
            }
            else if (bodyInspectionHandlerOption == null && option is BodyInspectionHandlerOption bodyInspectionOption)
            {
                bodyInspectionHandlerOption = bodyInspectionOption;
            }
        }

        return
        [
            uriReplacementOption != null ? new UriReplacementHandler<UriReplacementHandlerOption>(uriReplacementOption) : new UriReplacementHandler<UriReplacementHandlerOption>(),
            retryHandlerOption != null ? new RetryHandler(retryHandlerOption) : new RetryHandler(),
            redirectHandlerOption != null ? new RedirectHandler(redirectHandlerOption) : new RedirectHandler(),
            parametersNameDecodingOption != null ? new ParametersNameDecodingHandler(parametersNameDecodingOption) : new ParametersNameDecodingHandler(),
            userAgentHandlerOption != null ? new UserAgentHandler(userAgentHandlerOption) : new UserAgentHandler(),
            headersInspectionHandlerOption != null ? new HeadersInspectionHandler(headersInspectionHandlerOption) : new HeadersInspectionHandler(),
            bodyInspectionHandlerOption != null ? new BodyInspectionHandler(bodyInspectionHandlerOption) : new BodyInspectionHandler(),
        ];
    }

    /// <summary>
    ///   Gets the default handler types.
    /// </summary>
    /// <returns>A list of all the default handlers</returns>
    /// <remarks>Order matters</remarks>
    public static IList<ActivatableType> GetDefaultHandlerActivatableTypes()
    {
        return
        [
            new(typeof(UriReplacementHandler<UriReplacementHandlerOption>)),
            new(typeof(RetryHandler)),
            new(typeof(RedirectHandler)),
            new(typeof(ParametersNameDecodingHandler)),
            new(typeof(UserAgentHandler)),
            new(typeof(HeadersInspectionHandler)),
            new(typeof(BodyInspectionHandler)),
        ];
    }

    /// <summary>
    ///   Provides DI-safe trim annotations for an underlying type.
    ///   Required due to https://github.com/dotnet/runtime/issues/110239
    /// </summary>
    /// <param name="type">The type to be wrapped.</param>
    public readonly struct ActivatableType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
    {

        /// <summary>
        ///   The underlying type.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public readonly Type Type = type;

        /// <summary>
        ///   Implicitly converts from the wrapper to the underlying type.
        /// </summary>
        /// <param name="type">An instance of <see cref="ActivatableType"/></param>
        /// <returns>The <see cref="Type"/></returns>
        public static implicit operator Type(ActivatableType type) => type.Type;
    }

    /// <summary>
    ///   Creates a <see cref="DelegatingHandler"/> to use for the <see cref="HttpClient" /> from the provided <see cref="DelegatingHandler"/> instances. Order matters.
    /// </summary>
    /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects </param>
    /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
    /// <returns>The created <see cref="DelegatingHandler"/>.</returns>
    public static DelegatingHandler? ChainHandlersCollectionAndGetFirstLink(HttpMessageHandler? finalHandler, params DelegatingHandler[] handlers)
    {
        if (handlers == null || handlers.Length == 0)
        {
            return default;
        }

        int handlersCount = handlers.Length;
        for (int i = 0; i < handlersCount; i++)
        {
            DelegatingHandler handler = handlers[i];
            int previousItemIndex = i - 1;
            if (previousItemIndex >= 0)
            {
                DelegatingHandler previousHandler = handlers[previousItemIndex];
                previousHandler.InnerHandler = handler;
            }
        }
        if (finalHandler != null)
        {
            handlers[handlers.Length - 1].InnerHandler = finalHandler;
        }

        return handlers[0];//first
    }

    /// <summary>
    ///   Creates a <see cref="DelegatingHandler"/> to use for the <see cref="HttpClient" /> from the provided <see cref="DelegatingHandler"/> instances. Order matters.
    /// </summary>
    /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
    /// <returns>The created <see cref="DelegatingHandler"/>.</returns>
    public static DelegatingHandler? ChainHandlersCollectionAndGetFirstLink(params DelegatingHandler[] handlers)
    {
        return ChainHandlersCollectionAndGetFirstLink(null, handlers);
    }

    /// <summary>
    ///   Gets a default Http Client handler with the appropriate proxy configurations
    /// </summary>
    /// <param name="proxy">The proxy to be used with created client.</param>
    /// <returns/>
    public static HttpMessageHandler GetDefaultHttpMessageHandler(IWebProxy? proxy = null)
    {
#if BROWSER
        return new HttpClientHandler { AllowAutoRedirect = false };
#else
        return new SocketsHttpHandler { Proxy = proxy, AllowAutoRedirect = false, EnableMultipleHttp2Connections = true, AutomaticDecompression = DecompressionMethods.All };
#endif
    }
}

