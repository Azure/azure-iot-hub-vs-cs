using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "VisualC+!WindowsAppContainer")]
    internal class CppHandler : GenericAzureIoTHubServiceHandler
    {
        protected override HandlerManifest BuildHandlerManifest(ConnectedServiceHandlerContext context)
        {
            HandlerManifest manifest = new HandlerManifest();

            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.C.SharedUtility", "1.0.1"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.IoTHub.AmqpTransport", "1.0.1"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.IoTHub.IoTHubClient", "1.0.0"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.uamqp", "1.0.1"));

            manifest.Files.Add(new FileToAdd("CPP/NonWAC/azure_iot_hub.cpp"));
            manifest.Files.Add(new FileToAdd("CPP/NonWAC/azure_iot_hub.h"));

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

