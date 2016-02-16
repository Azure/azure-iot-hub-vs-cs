// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class AzureError
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
