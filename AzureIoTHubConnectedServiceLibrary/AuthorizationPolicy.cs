using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class AuthorizationPolicy
    {
        [DataMember(Name = "keyName")]
        public string KeyName { get; set; }

        [DataMember(Name = "primaryKey")]
        public string PrimaryKey { get; set; }

        [DataMember(Name = "secondaryKey")]
        public string SecondaryKey { get; set; }
    }

    [DataContract]
    internal class AuthorizationPolicies
    {
        [DataMember(Name = "value")]
        public AuthorizationPolicy[] Policies { get; set; }
    }
}
