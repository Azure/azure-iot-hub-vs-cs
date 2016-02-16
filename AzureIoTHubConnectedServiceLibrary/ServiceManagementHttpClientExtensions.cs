// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace AzureIoTHubConnectedService
{
    internal static class ServiceManagementHttpClientExtensions
    {
        public static async Task<T> PostEmptyBodyAsync<T>(this ServiceManagementHttpClient client, string relativeUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.PostAsync(relativeUri, null, cancellationToken).ConfigureAwait(false);
            await client.EnsureSuccessOrThrow(response).ConfigureAwait(false);
            return await client.ReadContentAs<T>(response).ConfigureAwait(false);
        }

        public static async Task EnsureSuccessOrThrow(this ServiceManagementHttpClient client, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // if it is not successful, try to get the exception message from the response
                AzureErrorResponse errorResponse = await client.ReadContentAs<AzureErrorResponse>(response);
                string code = errorResponse?.Error?.Code;
                string message = errorResponse?.Error?.Message;
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(message))
                {
                    if (code == "DisallowedProvider")
                    {
                        throw new InvalidOperationException("AzureResourceManagementResources.DisallowedProviderErrorMessge");
                    }

                    throw new InvalidOperationException(message);
                }
            }

            // if we didn't get an exception message from the response, just ensure a successful status code
            response.EnsureSuccessStatusCode();
        }

        private const int MaxRetryAttempts = 5;

        private const string IoTHubPreviewApiVersion = "2015-08-15-preview";

        public static Task<IoTHubListResponse> GetIoTHubsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                               "subscriptions/{0}/providers/Microsoft.Devices/IoTHubs?api-version={1}",
                                               client.SubscriptionId,
                                               IoTHubPreviewApiVersion);

            return client.GetAsync<IoTHubListResponse>(relativeUrl, cancellationToken);
        }

        public static async Task<IoTHub> GetIoTHubDetailsAsync(this ServiceManagementHttpClient client, IoTHub iotHubAccount, CancellationToken cancellationToken)
        {
            /// POST:
            ///  subscriptions/{subscriptionId}/resourceGroups/
            ///  {resourceGroupName}/providers/Microsoft.Devices/IotHubs/
            ///  {IotHubName}/IoTHubKeys/listkeys?api-version={api-version}

            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                   "subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Devices/IotHubs/{2}/IoTHubKeys/listKeys?api-version={3}",
                                   client.SubscriptionId,
                                   Uri.EscapeDataString(iotHubAccount.ResourceGroup),
                                   Uri.EscapeDataString(iotHubAccount.Name),
                                   IoTHubPreviewApiVersion);

            AuthorizationPolicies authorizationPolicies = await client.PostEmptyBodyAsync<AuthorizationPolicies>(relativeUrl, cancellationToken).ConfigureAwait(false);
            iotHubAccount.AuthorizationPolicies = authorizationPolicies;
            return iotHubAccount;
        }
    }
}
