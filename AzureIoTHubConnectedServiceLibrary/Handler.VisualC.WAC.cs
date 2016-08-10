// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
#if false // Disabled to a bug: https://github.com/Azure/azure-iot-sdks/issues/289
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "VisualC+WindowsAppContainer")]
#endif
    internal class CppHandlerWAC : GenericAzureIoTHubServiceHandler
    {
        protected override HandlerManifest BuildHandlerManifest(bool useTPM)
        {
            if (useTPM)
            {
                throw new NotSupportedException("TPM support for this project type is not yet supported");
            }

            HandlerManifest manifest = new HandlerManifest();

            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "9.0.1"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Amqp", "1.1.6"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Client", "1.0.16"));

            manifest.Files.Add(new FileToAdd("CPP/WAC/azure_iot_hub.cpp"));
            manifest.Files.Add(new FileToAdd("CPP/WAC/azure_iot_hub.h"));

            return manifest;
        }

        protected override AddServiceInstanceResult CreateAddServiceInstanceResult(ConnectedServiceHandlerContext context)
        {
            return new AddServiceInstanceResult(
                "",
                null
                );
        }

        protected override ConnectedServiceHandlerHelper GetConnectedServiceHandlerHelper(ConnectedServiceHandlerContext context)
        {
            return new AzureIoTHubConnectedServiceHandlerHelper(context);
        }
    }
}

