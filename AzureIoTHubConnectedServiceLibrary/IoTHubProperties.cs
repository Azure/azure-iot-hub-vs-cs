// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

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
