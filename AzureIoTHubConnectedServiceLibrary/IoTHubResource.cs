using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace AzureIoTHubConnectedService
{
    internal sealed class IoTHubResource : AzureResource, IAzureIoTHub
    {
        private readonly IAzureRMSubscription _subscription;
        private readonly IoTHub _iotHub;
        private readonly IReadOnlyDictionary<string, string> _properties;

        public IoTHubResource(IAzureRMSubscription subscription, IoTHub iotHubAccount)
        {
            _subscription = Arguments.ValidateNotNull(subscription, nameof(subscription));
            _iotHub = Arguments.ValidateNotNull(iotHubAccount, nameof(iotHubAccount));

            _properties = new Dictionary<string, string>()
            {
                { "IoTHubName", iotHubAccount.Name },
                { "Region", iotHubAccount.Location },
                { "SubscriptionName", subscription.SubscriptionName },
                { "ResourceGroup", iotHubAccount.ResourceGroup },
                { "Tier", iotHubAccount.Tier() },
                { "iotHubUri", iotHubAccount.Properties.HostName },
            };
        }

        public override string Id
        {
            get { return _iotHub.Id; }
        }

        public override IReadOnlyDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public async Task<PrimaryKeys> GetPrimaryKeysAsync(CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(_subscription);
            var client = await builder.CreateAsync().ConfigureAwait(false);

            var detailedAccount = await client.GetIoTHubDetailsAsync(_iotHub, cancellationToken);

            var keys = new PrimaryKeys {
                IoTHubOwner = detailedAccount.GetIoTHubOwnerPrimaryKey(),
                Service = detailedAccount.GetServicePrimaryKey()
            };

            return keys;
        }
    }
}
