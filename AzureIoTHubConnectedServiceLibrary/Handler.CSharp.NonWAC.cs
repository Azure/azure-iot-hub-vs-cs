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
            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "6.0.8"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Buffers-signed", "0.2.2"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs-signed", "0.2.2"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Codecs.Mqtt-signed", "0.2.2"));
            manifest.PackageReferences.Add(new NuGetReference("DotNetty.Common-signed", "0.2.2"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Amqp", "1.0.0"));
            manifest.PackageReferences.Add(new NuGetReference("System.Net.Http.Formatting.Extension", "5.2.3.0"));
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

