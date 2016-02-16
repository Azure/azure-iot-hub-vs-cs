// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    public interface IAzureResource : IEquatable<IAzureResource>
    {
        string Id { get; }
        IReadOnlyDictionary<string, string> Properties { get; }
    }

    public struct PrimaryKeys
    {
        public string IoTHubOwner;
        public string Service;
    }

    public interface IAzureIoTHub : IAzureResource
    {
        Task<PrimaryKeys> GetPrimaryKeysAsync(CancellationToken cancellationToken);
    }

    public interface IAzureIoTHubAccountManager
    {
        Task<IEnumerable<IAzureIoTHub>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken);

        Task<IAzureIoTHub> CreateIoTHubAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken);
    }
}