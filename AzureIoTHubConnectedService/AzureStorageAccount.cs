using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    internal sealed class AzureStorageAccount : AzureResource, IAzureStorageAccount
    {
        private readonly IAzureRMSubscription _subscription;
        private readonly StorageAccount _storageAccount;
        private readonly IReadOnlyDictionary<string, string> _properties;

        public AzureStorageAccount(IAzureRMSubscription subscription, StorageAccount storageAccount)
        {
            _subscription = Arguments.ValidateNotNull(subscription, nameof(subscription));
            _storageAccount = Arguments.ValidateNotNull(storageAccount, nameof(storageAccount));

            _properties = new Dictionary<string, string>()
            {
                { "StorageAccountName", storageAccount.Name },
                { "StorageAccountRegion", storageAccount.GetRegion() },
                { "SubscriptionName", subscription.SubscriptionName },
            };
        }

        public override string Id
        {
            get { return _storageAccount.Id; }
        }

        public override IReadOnlyDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public async Task<string> GetPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(_subscription);
            var client = await builder.CreateAsync().ConfigureAwait(false);

            StorageAccount detailedAccount = await client.GetStorageAccountDetailsAsync(_storageAccount, cancellationToken);
            return detailedAccount.GetPrimaryKey();
        }
    }
}
