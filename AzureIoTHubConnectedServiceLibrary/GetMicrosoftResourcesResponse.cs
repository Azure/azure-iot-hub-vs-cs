using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class GetMicrosoftResourcesResponse
    {
        [DataMember(Name = "resourceTypes")]
        public ResourceTypeInfo[] ResourceTypes { get; set; }
    }
}
