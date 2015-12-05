using System;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class StorageAccount
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "properties")]
        public StorageAccountProperties Properties { get; set; }

        public StorageKeys StorageServiceKeys { get; set; }

        public bool IsClassicStorage
        {
            get { return string.Equals(this.Type, "Microsoft.ClassicStorage/storageAccounts", StringComparison.OrdinalIgnoreCase); }
        }

        public string GetRegion()
        {
            return this.IsClassicStorage ? this.Properties.GeoPrimaryRegion : this.Properties.PrimaryLocation;
        }

        public string GetPrimaryKey()
        {
            return this.IsClassicStorage ? this.StorageServiceKeys.PrimaryKey : this.StorageServiceKeys.Key1;
        }
    }
}