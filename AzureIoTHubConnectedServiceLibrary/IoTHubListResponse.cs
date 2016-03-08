// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class IoTHubListResponse
    {
        [DataMember(Name = "value")]
        public IList<IoTHub> Accounts { get; set; }

        public IoTHubListResponse()
        {
            Accounts = new List<IoTHub>();
        }
    }
}
