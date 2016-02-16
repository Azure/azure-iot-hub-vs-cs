// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class AzureErrorResponse
    {
        [DataMember(Name = "error")]
        public AzureError Error { get; set; }
    }
}
