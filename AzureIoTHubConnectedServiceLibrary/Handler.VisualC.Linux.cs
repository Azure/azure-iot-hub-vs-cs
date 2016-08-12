// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using NuGet.VisualStudio;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "LinuxNative")]
    internal class CppLinuxHandler : ConnectedServiceHandler
    {
        [Import]
        private IVsPackageInstaller PackageInstaller { get; set; }

        [Import]
        private IVsPackageInstallerServices PackageInstallerServices { get; set; }

        public override async Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            var cancel = context.ServiceInstance.Metadata["Cancel"];
            if (cancel != null)
            {
                if ((bool)cancel)
                {
                    // Cancellation
                    throw new OperationCanceledException();
                }
            }

            AddServiceInstanceResult result = new AddServiceInstanceResult("", null);

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "New service instance {0} for Linux created", context.ServiceInstance.Name);

            return result;
        }
    }
}

