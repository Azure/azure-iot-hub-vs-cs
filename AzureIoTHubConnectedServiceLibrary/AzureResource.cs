// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace AzureIoTHubConnectedService
{
    public abstract class AzureResource : IAzureResource
    {
        #region IAzureResource

        public abstract string Id { get; }

        public abstract IReadOnlyDictionary<string, string> Properties { get; }

        public bool Equals(IAzureResource other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(this.Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAzureResource);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Id);
        }

        #endregion IAzureResource
    }
}
