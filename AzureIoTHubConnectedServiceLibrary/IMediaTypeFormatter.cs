// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System.Net.Http.Headers;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// A simplified abstraction of System.Net.Http.Formatting.MediaTypeFormatter.
    /// </summary>
    /// <remarks>
    /// This should be replaced by the System.Net.Http.Formatting.MediaTypeFormatter family
    /// if we decide to take dependency on System.Net.Http.Formatting.dll.
    /// </remarks>
    public interface IMediaTypeFormatter
    {
        /// <summary>
        /// The Accept header for an HTTP request.
        /// </summary>
        MediaTypeWithQualityHeaderValue Accept { get; }

        /// <summary>
        /// The Content-Type content header on an HTTP response.
        /// </summary>
        MediaTypeHeaderValue ContentType { get; }

        /// <summary>
        /// Serializes the given value of T as a string.
        /// </summary>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes the given string as an object of T.
        /// </summary>
        T Deserialize<T>(string value);
    }
}
