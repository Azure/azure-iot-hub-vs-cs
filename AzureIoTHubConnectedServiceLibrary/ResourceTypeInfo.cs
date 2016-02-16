// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class ResourceTypeInfo
    {
        [DataMember(Name = "resourceType")]
        public string ResourceType { get; set; }

        [DataMember(Name = "locations")]
        public IList<string> Locations { get; set; }
    }
}
