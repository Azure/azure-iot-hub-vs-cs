using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    internal sealed class IoTHubResource : AzureResource, IAzureIoTHub
    {
        private readonly IAzureRMSubscription _subscription;
        private readonly IoTHub _storageAccount;
        private readonly IReadOnlyDictionary<string, string> _properties;

        public IoTHubResource(IAzureRMSubscription subscription, IoTHub storageAccount)
        {
            _subscription = Arguments.ValidateNotNull(subscription, nameof(subscription));
            _storageAccount = Arguments.ValidateNotNull(storageAccount, nameof(storageAccount));

            _properties = new Dictionary<string, string>()
            {
                { "IoTHubName", storageAccount.Name },
                { "Region", storageAccount.Location },
                { "SubscriptionName", subscription.SubscriptionName },
                { "ResourceGroup", storageAccount.ResourceGroup },
                { "Tier", storageAccount.Tier() },
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

            var detailedAccount = await client.GetStorageAccountDetailsAsync(_storageAccount, cancellationToken);
            return detailedAccount.GetPrimaryKey();
        }
    }
}
