using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class Auth
    {
        [DataMember(Name = "symKey")]
        public SymKey SymKey { get; set; }
    }

    [DataContract]
    internal class SymKey
    {
        [DataMember(Name = "primary")]
        public string PrimaryKey { get; set; }

        [DataMember(Name = "secondary")]
        public string SecondaryKey { get; set; }
    }

        [DataContract]
    internal class DeviceIdentity
    {
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "auth")]
        public Auth Auth { get; set; }

    }
}
