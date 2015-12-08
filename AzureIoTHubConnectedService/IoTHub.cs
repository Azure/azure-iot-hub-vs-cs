using System;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class Sku
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tier")]
        public string Tier { get; set; }

        [DataMember(Name = "capacity")]
        public string Capacity { get; set; }
    }

    [DataContract]
    internal class IoTHub
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "subscriptionid")]
        public string SubscriptionId { get; set; }

        [DataMember(Name = "resourcegroup")]
        public string ResourceGroup { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "properties")]
        public IoTHubProperties Properties { get; set; }

        [DataMember(Name = "sku")]
        public Sku Sku { get; set; }

        public StorageKeys StorageServiceKeys { get; set; }

        public string Tier()
        {
            return string.Format("{0} {1}", this.Sku.Name, this.Sku.Tier);
        }

        public string GetPrimaryKey()
        {
            return this.StorageServiceKeys.Key1;
        }
    }
}