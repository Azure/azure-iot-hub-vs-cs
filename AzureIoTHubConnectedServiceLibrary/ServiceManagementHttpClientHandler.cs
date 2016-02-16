// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    internal sealed class ServiceManagementHttpClientHandler : DelegatingHandler
    {
        public ProductInfoHeaderValue UserAgent { get; set; }

        public string MsVersion { get; set; }

        public string AcceptLanguage { get; set; }

        public AuthenticationHeaderValue Authorization { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.UserAgent != null)
            {
                request.Headers.UserAgent.Add(this.UserAgent);
            }

            if (!string.IsNullOrEmpty(this.MsVersion))
            {
                request.Headers.Add("x-ms-version", this.MsVersion);
            }

            if (!string.IsNullOrEmpty(this.AcceptLanguage))
            {
                request.Headers.AcceptLanguage.TryParseAdd(this.AcceptLanguage);
            }

            if (this.Authorization != null)
            {
                request.Headers.Authorization = this.Authorization;
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
