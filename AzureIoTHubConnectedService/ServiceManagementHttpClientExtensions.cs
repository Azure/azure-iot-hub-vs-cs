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
        public static async Task PollForCompletion(this ServiceManagementHttpClient client, string operationStatusLink, int delayInSeconds, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));
            Arguments.ValidateNotNullOrWhitespace(operationStatusLink, nameof(operationStatusLink));

            bool done = false;
            while (!done)
            {
                HttpResponseMessage response = await client.GetAsync(operationStatusLink, cancellationToken).ConfigureAwait(false);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        done = true;
                        break;
                    case HttpStatusCode.Accepted:
                        int? retryAfter = response.GetRetryAfter();
                        if (retryAfter.HasValue)
                        {
                            delayInSeconds = retryAfter.Value;
                        }

                        TimeSpan pollInterval = new TimeSpan(0, 0, delayInSeconds);
                        await Task.Delay(pollInterval, cancellationToken);
                        break;
                    case HttpStatusCode.InternalServerError:
                        throw new InvalidOperationException("AzureResourceManagementResources.OperationFailed");
                    default:
                        throw new InvalidOperationException("Unexpected Operation Status");
                }
            }
        }

        public static int? GetRetryAfter(this HttpResponseMessage response)
        {
            int? result = null;
            if (response.Headers.Contains("RetryAfter"))
            {
                int headerValue;
                if (int.TryParse(response.Headers.GetValues("RetryAfter").FirstOrDefault(), out headerValue))
                {
                    result = headerValue;
                }
            }
            return result;
        }

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


        private const int DefaultDelayInSeconds = 25; // this value comes from https://github.com/Azure/azure-sdk-for-net/blob/master/src/ResourceManagement/Storage/StorageManagement/Generated/StorageAccountOperations.cs
        private const int MaxRetryAttempts = 5;

        private const string V1ApiVersion = "2014-06-01";
        private const string V2ApiVersion = "2015-05-01-preview";

        private const string Api_2015_01_01 = "2015-01-01";

        public static async Task<IEnumerable<ResourceGroup>> GetResourceGroupsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));

            string relativeUrl = $"subscriptions/{client.SubscriptionId}/resourcegroups?api-version={Api_2015_01_01}";

            GetResourceGroupsResponse response = await client.GetAsync<GetResourceGroupsResponse>(relativeUrl, cancellationToken);
            return response?.ResourceGroups ?? Enumerable.Empty<ResourceGroup>();
        }

        public static async Task CreateResourceGroupAsync(this ServiceManagementHttpClient client, string resourceGroupName, ResourceGroupCreationSettings settings, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.PutAsync($"/subscriptions/{client.SubscriptionId}/resourcegroups/{Uri.EscapeDataString(resourceGroupName)}?api-version={Api_2015_01_01}", settings, cancellationToken);
            await client.EnsureSuccessOrThrow(response);
        }

        public static async Task<IEnumerable<ResourceLocation>> GetLocationsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            var getResourcesTask = client.GetAsync<GetMicrosoftResourcesResponse>($"/subscriptions/{client.SubscriptionId}/providers/Microsoft.Resources?api-version={Api_2015_01_01}", cancellationToken);

            //Get all possible resource group locations
            var getAllLocationsTask = GetAllLocationsAsync(client, cancellationToken);

            await Task.WhenAll(new Task[] { getResourcesTask, getAllLocationsTask }).ConfigureAwait(false);

            var response = getResourcesTask.Result;
            IEnumerable<ResourceLocation> allLocations = getAllLocationsTask.Result;

            if (response.ResourceTypes != null)
            {
                foreach (ResourceTypeInfo resourceType in response.ResourceTypes)
                {
                    if (resourceType.ResourceType == "resourceGroups" && resourceType.Locations != null)
                    {
                        List<ResourceLocation> locations = new List<ResourceLocation>();
                        foreach (var location in resourceType.Locations)
                        {
                            locations.Add(new ResourceLocation() { Name = location, DisplayName = location });
                        }

                        //Update the display name for those locations that match with the values in allLocations
                        foreach (ResourceLocation validLocation in locations)
                        {
                            var matchingLocation = allLocations.FirstOrDefault(lo => String.Equals(lo.Name, validLocation.Name, StringComparison.OrdinalIgnoreCase));
                            if (matchingLocation != null)
                            {
                                validLocation.DisplayName = matchingLocation.DisplayName;
                            }
                            else
                            {
                                var matchingLocationViaDisplayName = allLocations.FirstOrDefault(lo => String.Equals(lo.DisplayName, validLocation.Name, StringComparison.OrdinalIgnoreCase));
                                if (matchingLocationViaDisplayName != null)
                                {
                                    validLocation.Name = matchingLocationViaDisplayName.Name;
                                    validLocation.DisplayName = matchingLocationViaDisplayName.DisplayName;
                                }
                                else
                                {
                                    Debug.Fail("Could not find a valid location with the given name");
                                }
                            }
                        }
                        return locations.ToArray();
                    }
                }
            }

            Debug.Fail("Couldn't get locations");
            return Enumerable.Empty<ResourceLocation>();
        }

        private static async Task<IEnumerable<ResourceLocation>> GetAllLocationsAsync(ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync<GetResourceLocationsResponse>($"/subscriptions/{client.SubscriptionId}/locations?api-version={Api_2015_01_01}", cancellationToken);
            if (response.Locations != null)
            {
                ResourceLocation[] resourceLocations = response.Locations;

                foreach (ResourceLocation resourceLocation in resourceLocations)
                {
                    if (resourceLocation.DisplayName == null)
                    {
                        resourceLocation.DisplayName = resourceLocation.Name;
                    }
                }

                return resourceLocations;
            }

            Debug.Fail("Couldn't get locations");
            return Enumerable.Empty<ResourceLocation>();
        }

        public static async Task<IEnumerable<string>> GetStorageLocationsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));

            string relativeUrl = $"subscriptions/{client.SubscriptionId}/providers/Microsoft.Storage?api-version={Api_2015_01_01}";

            ResourceProvider providerInfo = await client.GetAsync<ResourceProvider>(relativeUrl, cancellationToken);

            ResourceTypeInfo resourceTypeInfo = providerInfo.ResourceTypes.FirstOrDefault(t => t.ResourceType == "storageAccounts");
            return resourceTypeInfo?.Locations;
        }

        public static Task<StorageAccountListResponse> GetV1StorageAccountsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            return client.GetStorageAccountsAsync("Microsoft.ClassicStorage", V1ApiVersion, cancellationToken);
        }

        public static Task<StorageAccountListResponse> GetV2StorageAccountsAsync(this ServiceManagementHttpClient client, CancellationToken cancellationToken)
        {
            return client.GetStorageAccountsAsync("Microsoft.Storage", V2ApiVersion, cancellationToken);
        }

        private static Task<StorageAccountListResponse> GetStorageAccountsAsync(this ServiceManagementHttpClient client, string resourceProvider, string apiVersion, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));

            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                               "subscriptions/{0}/providers/{1}/storageaccounts?api-version={2}",
                                               client.SubscriptionId,
                                               resourceProvider,
                                               apiVersion);

            return client.GetAsync<StorageAccountListResponse>(relativeUrl, cancellationToken);
        }

        public static async Task<StorageAccount> GetStorageAccountDetailsAsync(this ServiceManagementHttpClient client, StorageAccount storageAccount, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));

            if (storageAccount == null)
            {
                return null;
            }

            string apiVersion = storageAccount.IsClassicStorage ? V1ApiVersion : V2ApiVersion;

            string relativeUrl = string.Format(CultureInfo.InvariantCulture,
                                   "{0}/listKeys?api-version={1}",
                                   storageAccount.Id,
                                   apiVersion);

            StorageKeys storageKeys = await client.PostEmptyBodyAsync<StorageKeys>(relativeUrl, cancellationToken).ConfigureAwait(false);
            storageAccount.StorageServiceKeys = storageKeys;
            return storageAccount;
        }

        public static async Task<StorageAccount> CreateStorageAccountAsync(this ServiceManagementHttpClient client, CreateStorageAccountInput input, CancellationToken cancellationToken)
        {
            Arguments.ValidateNotNull(client, nameof(client));
            Arguments.ValidateNotNull(input, nameof(input));

            string storageAccountUrl = $"subscriptions/{client.SubscriptionId}/resourceGroups/{Uri.EscapeDataString(input.ResourceGroupName)}/providers/Microsoft.Storage/storageAccounts/{Uri.EscapeDataString(input.AccountName)}?api-version={V2ApiVersion}";
            HttpResponseMessage response = await client.CreateStorageAccountAsync(storageAccountUrl, input, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                string operationStatusLink;
                IEnumerable<string> headerValues;
                if (response.Headers.TryGetValues("Location", out headerValues))
                {
                    operationStatusLink = headerValues.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(operationStatusLink))
                    {
                        int delayInSeconds = DefaultDelayInSeconds;
                        int? retryAfter = response.GetRetryAfter();
                        if (retryAfter.HasValue)
                        {
                            delayInSeconds = retryAfter.Value;
                        }

                        await client.PollForCompletion(operationStatusLink, delayInSeconds, cancellationToken);
                    }
                }
            }

            return await client.GetAsync<StorageAccount>(storageAccountUrl, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> CreateStorageAccountAsync(this ServiceManagementHttpClient client, string storageAccountUrl, CreateStorageAccountInput input, CancellationToken cancellationToken, int retryCount = 0)
        {
            HttpResponseMessage response = await client.PutAsync(storageAccountUrl, input, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.Conflict && retryCount < MaxRetryAttempts)
            {
                retryCount++;

                AzureErrorResponse errorResponse = await client.ReadContentAs<AzureErrorResponse>(response);
                string code = errorResponse?.Error?.Code;
                if (code == "MissingSubscriptionRegistration")
                {
                    // if we get an error for a missing registration, try registering the provider namespace and then creating the storage account again
                    await client.RegisterResourceProviderAsync("Microsoft.Storage", cancellationToken).ConfigureAwait(false);
                    response = await client.CreateStorageAccountAsync(storageAccountUrl, input, cancellationToken, retryCount).ConfigureAwait(false);
                }
                else if (code == "SubscriptionOperationInProgress")
                {
                    // if we get this error, wait for some time and try again
                    TimeSpan delay = new TimeSpan(0, 0, DefaultDelayInSeconds);
                    await Task.Delay(delay, cancellationToken);
                    response = await client.CreateStorageAccountAsync(storageAccountUrl, input, cancellationToken, retryCount).ConfigureAwait(false);
                }
            }
            await client.EnsureSuccessOrThrow(response);
            return response;
        }

        private static async Task RegisterResourceProviderAsync(this ServiceManagementHttpClient client, string resourceProviderNamespace, CancellationToken cancellationToken)
        {
            string registerUrl = $"subscriptions/{client.SubscriptionId}/providers/{resourceProviderNamespace}/register?api-version={Api_2015_01_01}";

            // Empty body for this particular POST action.
            HttpResponseMessage response = await client.PostAsync(registerUrl, null, cancellationToken).ConfigureAwait(false);
            await client.EnsureSuccessOrThrow(response).ConfigureAwait(false);
        }


    }
}
