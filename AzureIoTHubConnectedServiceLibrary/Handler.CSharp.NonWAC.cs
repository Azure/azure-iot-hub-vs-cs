// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "CSharp+!WindowsAppContainer")]
    internal class CSharpHandlerNonWAC : GenericAzureIoTHubServiceHandler
    {
        protected override HandlerManifest BuildHandlerManifest(ConnectedServiceHandlerContext context)
        {
            HandlerManifest manifest = new HandlerManifest();
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Buffers-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs.Mqtt-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Common-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Handlers-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Transport-signed", "0.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.AspNet.WebApi.Client", "5.2.3"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Amqp", "1.1.5"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Client", "1.0.8"));
            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "8.0.3"));

            manifest.PackageReferences.Add(new NuGetReference("PCLCrypto", "2.0.147"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.BCrypt", "0.3.2"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.Kernel32", "0.3.2"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.NCrypt", "0.3.2"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.Windows.Core", "0.3.2"));
            manifest.PackageReferences.Add(new NuGetReference("Validation", "2.2.8"));

            manifest.Files.Add(new FileToAdd("CSharp/AzureIoTHub.cs"));
            return manifest;
        }

        protected override AddServiceInstanceResult CreateAddServiceInstanceResult(ConnectedServiceHandlerContext context)
        {
            return new AddServiceInstanceResult(
                context.ServiceInstance.Name,
                new Uri("http://aka.ms/azure-iot-hub-vs-cs-cs")
                );
        }

        protected override ConnectedServiceHandlerHelper GetConnectedServiceHandlerHelper(ConnectedServiceHandlerContext context)
        {
            return context.HandlerHelper;
        }
    }
}

