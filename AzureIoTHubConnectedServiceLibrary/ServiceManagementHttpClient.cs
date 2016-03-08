// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public sealed class ServiceManagementHttpClient : HttpClient
    {
        private readonly IMediaTypeFormatter _formatter;
        private readonly string _subscriptionId;

        public string SubscriptionId
        {
            get { return _subscriptionId; }
        }

        internal ServiceManagementHttpClient(HttpMessageHandler handler, IMediaTypeFormatter formatter, string subscriptionId)
            : base(handler)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            _formatter = formatter;
            _subscriptionId = subscriptionId;
        }

        public async Task<T> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
            where T : new()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);

            request.Headers.Accept.Add(_formatter.Accept);

            var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) return new T();

            response.EnsureSuccessStatusCode();

            return await ReadContentAs<T>(response);
        }

        public Task<HttpResponseMessage> PutAsync<T>(string relativeUri, T requestBody, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, relativeUri);

            request.Headers.Accept.Add(_formatter.Accept);

            request.Content = new StringContent(_formatter.Serialize<T>(requestBody));

            request.Content.Headers.ContentType = _formatter.ContentType;

            return SendAsync(request, cancellationToken);
        }

        public async Task<T> ReadContentAs<T>(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return _formatter.Deserialize<T>(content);
        }
    }
}
