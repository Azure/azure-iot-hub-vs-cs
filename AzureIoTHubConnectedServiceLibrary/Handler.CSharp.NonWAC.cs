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
        protected override HandlerManifest BuildHandlerManifest(bool useTPM)
        {
            HandlerManifest manifest = new HandlerManifest();
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Buffers", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs.Mqtt", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Common", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Handlers", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Transport", "0.4.4"));
            manifest.PackageReferences.Add(new NuGetReference("EnterpriseLibrary.TransientFaultHandling", "6.0.1304.0"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.AspNet.WebApi.Client", "5.2.3"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Amqp", "2.0.5"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Client", "1.2.9"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Shared", "1.0.10"));
            manifest.PackageReferences.Add(new NuGetReference("Mono.Security", "3.2.3.0"));
            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "10.0.2"));
            manifest.PackageReferences.Add(new NuGetReference("PCLCrypto", "2.0.147"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.BCrypt", "0.5.64"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.Kernel32", "0.5.64"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.NCrypt", "0.5.64"));
            manifest.PackageReferences.Add(new NuGetReference("PInvoke.Windows.Core", "0.5.64"));
            manifest.PackageReferences.Add(new NuGetReference("Validation", "2.4.15"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Extensions.Logging", "1.1.1"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Extensions.Logging.Abstractions", "1.1.1"));

            if (useTPM)
            {
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.Devices.Tpm", "1.0.0"));
                manifest.PackageReferences.Add(new NuGetReference("Microsoft.TSS", "1.0.6"));
            }

            if (useTPM)
            {
                manifest.Files.Add(new FileToAdd("CSharp/Tpm/AzureIoTHub.cs"));
            }
            else
            {
                manifest.Files.Add(new FileToAdd("CSharp/AzureIoTHub.cs"));
            }

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

