using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class CreateStorageAccountProperties
    {
        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }
    }
}
