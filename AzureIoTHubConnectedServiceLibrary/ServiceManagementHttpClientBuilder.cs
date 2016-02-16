// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public sealed class ServiceManagementHttpClientBuilder
    {
        private readonly IAzureRMSubscription _azureRMSubscription;

        public ProductInfoHeaderValue UserAgent { get; set; }

        public string MsVersion { get; set; }

        public string AcceptLanguage { get; set; }

        public IMediaTypeFormatter Formatter { get; set; }

        public ServiceManagementHttpClientBuilder(IAzureRMSubscription azureRMSubscription)
        {
            if (azureRMSubscription == null)
            {
                throw new ArgumentNullException(nameof(azureRMSubscription));
            }

            _azureRMSubscription = azureRMSubscription;

            ApplyDefaultSettings();
        }

        public async Task<ServiceManagementHttpClient> CreateAsync()
        {
            if (this.UserAgent == null)
            {
                throw new InvalidOperationException("UserAgent is null.");
            }

            if (string.IsNullOrEmpty(this.MsVersion))
            {
                throw new InvalidOperationException("MsVersion is null or empty.");
            }

            if (this.Formatter == null)
            {
                throw new InvalidOperationException("Formatter is null.");
            }

            var webRequestHandler = new WebRequestHandler();

            //
            // Build a ServiceManagementHttpClientHandler.
            //
            var clientHandler = await BuildServiceManagementHttpClientHandlerAsync().ConfigureAwait(false);
            clientHandler.InnerHandler = webRequestHandler;

            Debug.Assert(_azureRMSubscription != null);
            string subscriptionId = _azureRMSubscription.SubscriptionId;

            //
            // Build a http client.
            //
            var client = new ServiceManagementHttpClient(clientHandler, this.Formatter, subscriptionId);
            client.BaseAddress = _azureRMSubscription.ResourceManagementEndpointUri;

            return client;
        }

        private async Task<ServiceManagementHttpClientHandler> BuildServiceManagementHttpClientHandlerAsync()
        {
            var clientHandler = new ServiceManagementHttpClientHandler();

            clientHandler.UserAgent = this.UserAgent;
            clientHandler.MsVersion = this.MsVersion;
            clientHandler.AcceptLanguage = this.AcceptLanguage;

            Debug.Assert(_azureRMSubscription != null);

            string authorization = await _azureRMSubscription.Tenant.GetAuthenticationHeaderAsync().ConfigureAwait(false);
            Debug.Assert(!string.IsNullOrEmpty(authorization));

            if (!string.IsNullOrEmpty(authorization))
            {
                clientHandler.Authorization = AuthenticationHeaderValue.Parse(authorization);
            }

            return clientHandler;
        }

        private void ApplyDefaultSettings()
        {
            this.UserAgent = ProductInfoHeaderValue.Parse("VisualStudio2015");
            this.MsVersion = "2013-11-01";
            this.AcceptLanguage = CultureInfo.CurrentUICulture.Name;

            // AzureRM should use JSON
            this.Formatter = JsonMediaTypeFormatter.Default;
        }
    }
}
