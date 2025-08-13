﻿namespace ReQuesty.Runtime.Http.Tests.Mocks
{
    public class TestHttpMessageHandler : DelegatingHandler
    {
        private readonly Action<HttpRequestMessage> requestMessageDelegate;
        private readonly Dictionary<string, HttpResponseMessage> responseMessages;

        public TestHttpMessageHandler(Action<HttpRequestMessage>? requestMessage = null)
        {
            this.requestMessageDelegate = requestMessage ?? DefaultRequestHandler;
            this.responseMessages = [];
        }

        public void AddResponseMapping(string requestUrl, HttpResponseMessage responseMessage)
        {
            this.responseMessages.Add(requestUrl, responseMessage);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requestMessageDelegate(request);

            if (this.responseMessages.TryGetValue(request.RequestUri!.ToString(), out HttpResponseMessage? responseMessage))
            {
                responseMessage.RequestMessage = request;
                return Task.FromResult(responseMessage);
            }

            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
        }

        private void DefaultRequestHandler(HttpRequestMessage httpRequest)
        {

        }
    }
}
