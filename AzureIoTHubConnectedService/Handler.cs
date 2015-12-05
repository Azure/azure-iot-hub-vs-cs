using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace IoTHubzConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "CSharp")]
    internal class Handler : ConnectedServiceHandler
    {
        public override Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            AddServiceInstanceResult result = new AddServiceInstanceResult(
                "Sample",
                new Uri("https://github.com/Microsoft/ConnectedServicesSdkSamples"));
            return Task.FromResult(result);
        }
    }
}

