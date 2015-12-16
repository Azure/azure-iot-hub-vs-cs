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
    public sealed class AzureIoTHubAccountManager : IAzureIoTHubAccountManager
    {
        public AzureIoTHubAccountManager()
        {
        }

        public async Task<IEnumerable<IAzureIoTHub>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            IoTHubListResponse response = await ServiceManagementHttpClientExtensions.GetIoTHubsAsync(client, cancellationToken).ConfigureAwait(false);

            return response.Accounts.Select(p => new IoTHubResource(subscription, p)).ToList();
        }

        public async Task<IAzureIoTHub> CreateStorageAccountAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken)
        {
            IAzureIoTHub result = null;
            try
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.Generic.InvokeAsync(() =>
                {
                    throw new NotImplementedException();
                    /*
                    using (CreateServiceViewModel viewModel = new CreateServiceViewModel(serviceProvider, userAccount))
                    {
                        using (CreateServiceDialog dialog = new CreateServiceDialog(viewModel))
                        {
                            dialog.ShowModal();

                            StorageAccount createdAccount = viewModel.CreatedStorageAccount;
                            if (createdAccount != null)
                            {
                                result = new AzureStorageAccount(viewModel.SubscriptionsViewModel.SelectedSubscription.UnderlyingSubscription, createdAccount);
                            }
                        }
                    }
                    */

                }).ConfigureAwait(false);

                return result;
            }
            catch (Exception /*ex*/)
            {
                // await ShellUtilities.ShowErrorMessageAsync("Error occurred during creation: " + ex.Message).ConfigureAwait(false);
                throw;
            }
        }

    }
}
