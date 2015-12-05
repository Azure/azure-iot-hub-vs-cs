using Microsoft.VisualStudio.Services.Client.AccountManagement;
//using Microsoft.VisualStudio.Services.Account;
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

    public interface IAzureStorageAccount : IAzureResource
    {
        Task<string> GetPrimaryKeyAsync(CancellationToken cancellationToken);
    }

    public interface IAzureIoTHubAccountManager
    {
        Task<IEnumerable<IAzureStorageAccount>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken);

        Task<IAzureStorageAccount> CreateStorageAccountAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken);
    }
}