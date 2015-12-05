using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class GetResourceLocationsResponse
    {
        [DataMember(Name = "value")]
        public ResourceLocation[] Locations { get; set; }
    }
}
