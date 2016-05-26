// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Services.Client.AccountManagement;
using System.Threading;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceProviderExport("Microsoft.AzureIoTHubService")]
    internal class AzureIoTHubProvider : ConnectedServiceProvider
    {
        [Import]
        private IAzureIoTHubAccountManager IoTHubAccountManager { get; set; }

        [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [ImportingConstructor]
        public AzureIoTHubProvider()
        {

            this.Category = "Microsoft";
            this.Name = "Azure IoT Hub";
            this.Description = "Communicate between IoT devices and the cloud.";
            this.Icon = new BitmapImage(new Uri("pack://application:,,/" + this.GetType().Assembly.ToString() + ";component/AzureIoTHubProviderIcon.png"));
            this.CreatedBy = "Microsoft";
            this.Version = new Version(1, 3, 0);
            this.MoreInfoUri = new Uri("http://aka.ms/iothubgetstartedVSCS");
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceConfigurator configurator;

            var securityMode = new TPMSelector();
            var dlgResult = securityMode.ShowModal();
            if (dlgResult.HasValue && dlgResult.Value)
            {
                var useTPM = securityMode.rbUseTPM.IsChecked;
                if (useTPM.HasValue && useTPM.Value)
                {
                    // The user has chosen to use TPM
                    configurator = new ConnectedServiceSansUI(false);
                }
                else
                {
                    // No TPM
                    configurator = new AzureIoTHubAccountProviderGrid(this.IoTHubAccountManager, this.ServiceProvider);
                }
            }
            else
            {
                // User cancelled
                configurator = new ConnectedServiceSansUI(true);
            }

            return Task.FromResult(configurator);
        }
    }

    class ConnectedServiceSansUI : ConnectedServiceUILess
    {
        ConnectedServiceInstance instance;

        public ConnectedServiceSansUI(bool cancel)
        {
            instance = new ConnectedServiceInstance();
            instance.Metadata.Add("Cancel", cancel);
            instance.Metadata.Add("TPM", true);
        }

        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            return Task.FromResult(instance);
        }
    }
}
