using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class CreateStorageAccountInput
    {
        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "properties")]
        public CreateStorageAccountProperties Properties { get; set; }

        // NOTE: the following members are not serialized

        public string AccountName { get; set; }

        public string ResourceGroupName { get; set; }
    }
}
