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
            this.Name = "Microsoft Azure IoT Connected Service";
            this.Description = "Communicate between IoT devices and the cloud.";
            this.Icon = new BitmapImage(new Uri("pack://application:,,/" + this.GetType().Assembly.ToString() + ";component/AzureIoTHubProviderIcon.png"));
            this.CreatedBy = "Microsoft";
            this.Version = new Version(1, 0, 0);
            this.MoreInfoUri = new Uri("http://aka.ms/iothubgetstartedVSCS");
        }
        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceConfigurator configurator = new AzureIoTHubAccountProviderGrid(this.IoTHubAccountManager, this.ServiceProvider);
            return Task.FromResult(configurator);
        }
    }
}
