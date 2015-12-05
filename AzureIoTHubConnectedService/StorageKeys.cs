using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class StorageKeys
    {
        // Microsoft.Storage keys
        [DataMember(Name = "key1")]
        public string Key1 { get; set; }

        [DataMember(Name = "key2")]
        public string Key2 { get; set; }

        // Microsoft.ClassicStorage keys
        [DataMember(Name = "primaryKey")]
        public string PrimaryKey { get; set; }

        [DataMember(Name = "secondaryKey")]
        public string SecondaryKey { get; set; }
    }
}
