using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System.Threading;

namespace AzureIoTHubConnectedService
{
    [Export(typeof(IAzureIoTHubAccountManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class AzureStorageAccountManager : IAzureIoTHubAccountManager
    {
        public AzureStorageAccountManager()
        {
        }

        public Task<IAzureStorageAccount> CreateStorageAccountAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IAzureStorageAccount>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
