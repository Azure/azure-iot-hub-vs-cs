using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class ResourceGroupCreationSettings
    {
        [DataMember(Name = "location")]
        public string Location { get; set; }
    }
}
