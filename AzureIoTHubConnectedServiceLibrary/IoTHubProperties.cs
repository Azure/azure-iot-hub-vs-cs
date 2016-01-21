using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class IoTHubProperties
    {
        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "provisioningState")]
        public string ProvisioningState { get; set; }

        [DataMember(Name = "hostName")]
        public string HostName { get; set; }

        [DataMember(Name = "cloudToDevice")]
        public CloudToDevice CloudToDevice { get; set; }
    }
}
