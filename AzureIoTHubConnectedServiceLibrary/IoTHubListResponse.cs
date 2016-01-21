using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class IoTHubListResponse
    {
        [DataMember(Name = "value")]
        public IList<IoTHub> Accounts { get; set; }
    }
}
