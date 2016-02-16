// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System.Threading;

namespace AzureIoTHubConnectedService
{
    [Export(typeof(IAzureIoTHubAccountManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class AzureIoTHubAccountManager : IAzureIoTHubAccountManager
    {
        public AzureIoTHubAccountManager()
        {
        }

        public async Task<IEnumerable<IAzureIoTHub>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            IoTHubListResponse response = await ServiceManagementHttpClientExtensions.GetIoTHubsAsync(client, cancellationToken).ConfigureAwait(false);

            return response.Accounts.Select(p => new IoTHubResource(subscription, p)).ToList();
        }

        public Task<IAzureIoTHub> CreateIoTHubAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
