// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AzureIoTHubConnectedService
{
    internal sealed class JsonMediaTypeFormatter : IMediaTypeFormatter
    {
        public static readonly JsonMediaTypeFormatter Default = new JsonMediaTypeFormatter();

        private readonly MediaTypeWithQualityHeaderValue _accept = MediaTypeWithQualityHeaderValue.Parse("application/json");

        private readonly MediaTypeHeaderValue _contentType = MediaTypeHeaderValue.Parse("application/json");

        public MediaTypeWithQualityHeaderValue Accept
        {
            get { return _accept; }
        }

        public MediaTypeHeaderValue ContentType
        {
            get { return _contentType; }
        }

        public string Serialize<T>(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer<T>.Serializer.WriteObject(stream, value);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public T Deserialize<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                T val = (T)DataContractSerializer<T>.Serializer.ReadObject(stream);
                return val;
            }
        }

        private static class DataContractSerializer<T>
        {
            public static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(T));
        }
    }
}
