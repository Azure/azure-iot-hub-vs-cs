using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Represents a location supported by the Resource Manager
    /// </summary>
    [DataContract]
    internal class ResourceLocation
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }
    }
}
