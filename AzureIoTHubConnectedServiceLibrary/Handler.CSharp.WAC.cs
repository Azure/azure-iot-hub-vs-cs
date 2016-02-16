// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "CSharp+WindowsAppContainer")]
    internal class CSharpHandlerWAC : GenericAzureIoTHubServiceHandler
    {
        protected override HandlerManifest BuildHandlerManifest(ConnectedServiceHandlerContext context)
        {
            HandlerManifest manifest = new HandlerManifest();

            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "6.0.8"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Client", "1.0.1"));
            manifest.Files.Add(new FileToAdd("CSharp/AzureIoTHub.cs"));
            return manifest;
        }

        protected override AddServiceInstanceResult CreateAddServiceInstanceResult(ConnectedServiceHandlerContext context)
        {
            return new AddServiceInstanceResult(
                context.ServiceInstance.Name,
                new Uri("http://aka.ms/azure-iot-connected-service-cs")
                );
        }

        protected override ConnectedServiceHandlerHelper GetConnectedServiceHandlerHelper(ConnectedServiceHandlerContext context)
        {
            return context.HandlerHelper;
        }
    }
}

