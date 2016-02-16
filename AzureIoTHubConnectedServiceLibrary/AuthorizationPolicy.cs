// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

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
