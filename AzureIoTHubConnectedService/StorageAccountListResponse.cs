using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class StorageAccountListResponse
    {
        [DataMember(Name = "value")]
        public IList<StorageAccount> Accounts { get; set; }
    }
}
