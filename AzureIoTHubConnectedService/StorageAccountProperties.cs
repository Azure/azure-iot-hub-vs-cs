using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class StorageAccountProperties
    {
        // Microsoft.Storage location
        [DataMember(Name = "primaryLocation")]
        public string PrimaryLocation { get; set; }

        // Microsoft.ClassicStorage location
        [DataMember(Name = "geoPrimaryRegion")]
        public string GeoPrimaryRegion { get; set; }
    }
}
