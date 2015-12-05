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
//using Microsoft.VisualStudio.Shell;
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

        public async Task<IEnumerable<IAzureStorageAccount>> EnumerateIoTHubAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            Task<IEnumerable<IAzureStorageAccount>> v1AccountsTask = this.EnumerateV1StorageAccountsAsync(subscription, cancellationToken);
            Task<IEnumerable<IAzureStorageAccount>> v2AccountsTask = this.EnumerateV2StorageAccountsAsync(subscription, cancellationToken);
            await Task.WhenAll(v1AccountsTask, v2AccountsTask).ConfigureAwait(false);

            return v1AccountsTask.Result.Concat(v2AccountsTask.Result);
        }

        private Task<IEnumerable<IAzureStorageAccount>> EnumerateV1StorageAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            return this.EnumerateStorageAccountsAsync(subscription, ServiceManagementHttpClientExtensions.GetV1StorageAccountsAsync, cancellationToken);
        }

        private Task<IEnumerable<IAzureStorageAccount>> EnumerateV2StorageAccountsAsync(IAzureRMSubscription subscription, CancellationToken cancellationToken)
        {
            return this.EnumerateStorageAccountsAsync(subscription, ServiceManagementHttpClientExtensions.GetV2StorageAccountsAsync, cancellationToken);
        }

        private async Task<IEnumerable<IAzureStorageAccount>> EnumerateStorageAccountsAsync(IAzureRMSubscription subscription, Func<ServiceManagementHttpClient, CancellationToken, Task<StorageAccountListResponse>> serverCall, CancellationToken cancellationToken)
        {
            var builder = new ServiceManagementHttpClientBuilder(subscription);

            var client = await builder.CreateAsync().ConfigureAwait(false);

            StorageAccountListResponse response = await serverCall(client, cancellationToken).ConfigureAwait(false);

            return response.Accounts.Select(p => new AzureStorageAccount(subscription, p)).ToList();
        }

        public async Task<IAzureStorageAccount> CreateStorageAccountAsync(IServiceProvider serviceProvider, Account userAccount, CancellationToken cancellationToken)
        {
            IAzureStorageAccount result = null;
            try
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.Generic.InvokeAsync(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Creating service model");
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
