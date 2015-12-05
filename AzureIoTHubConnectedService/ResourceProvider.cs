using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class ResourceProvider
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "resourceTypes")]
        public IList<ResourceTypeInfo> ResourceTypes { get; set; }
    }
}
