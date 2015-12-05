using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class GetResourceGroupsResponse
    {
        [DataMember(Name = "value")]
        public ResourceGroup[] ResourceGroups { get; set; }
    }
}
