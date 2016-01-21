using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    public abstract class AzureServiceAuthenticator : ConnectedServiceAuthenticator
    {
        private AccountPickerViewModel accountPickerViewModel;

        protected AzureServiceAuthenticator(IServiceProvider serviceProvider, string providerId)
        {
            this.accountPickerViewModel = new AccountPickerViewModel(serviceProvider, "ConnectedServices:" + providerId);
            this.accountPickerViewModel.PropertyChanged += this.AccountPickerViewModel_PropertyChanged;
            this.accountPickerViewModel.AuthenticationChanged += this.AccountPickerViewModel_AuthenticationChanged;
            this.CalculateIsAuthenticated();

            this.View = new AccountPicker(this.accountPickerViewModel);
        }

        public Task<Account> GetAccountAsync()
        {
            return this.accountPickerViewModel.GetAccountAsync();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.accountPickerViewModel.PropertyChanged -= this.AccountPickerViewModel_PropertyChanged;
                    this.accountPickerViewModel.AuthenticationChanged -= this.AccountPickerViewModel_AuthenticationChanged;
                    this.accountPickerViewModel.Dispose();

                    ((AccountPicker)this.View).Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected async Task<IEnumerable<IAzureSubscriptionContext>> GetSubscriptionContextsAsync()
        {
            IEnumerable<IAzureSubscriptionContext> subscriptions = Enumerable.Empty<IAzureSubscriptionContext>();

            Account account = await this.GetAccountAsync();
            if (account != null && !account.NeedsReauthentication)
            {
                IAzureUserAccount azureUserAccount = this.accountPickerViewModel.AuthenticationManager.UserAccounts.FirstOrDefault(a => a.UniqueId == account.UniqueId);

                if (azureUserAccount != null)
                {
                    try
                    {
                        subscriptions = await azureUserAccount.GetSubscriptionsAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // User cancelled out of the login prompt, etc. - ignore exception and return no subscriptions
                    }
                }
            }

            return subscriptions;
        }

        public virtual async Task<bool> SelectedAccountHasSubscriptions()
        {
            return (await this.GetSubscriptionContextsAsync().ConfigureAwait(false)).Any();
        }

        private void CalculateIsAuthenticated()
        {
            this.IsAuthenticated = this.accountPickerViewModel.IsAuthenticated;
        }

        private void AccountPickerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.accountPickerViewModel.IsAuthenticated))
            {
                this.CalculateIsAuthenticated();
            }
        }

        private void AccountPickerViewModel_AuthenticationChanged(object sender, EventArgs e)
        {
            // rebroadcast the AuthenticationChanged event whenever the underlying accountPickerViewModel
            // raises the AuthenticationChanged event
            this.OnAuthenticationChanged(new AuthenticationChangedEventArgs());
        }
    }

    internal class Authenticator : AzureServiceAuthenticator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAzureRMTenantService tenantService;

        public Authenticator(IServiceProvider serviceProvider)
            : base(serviceProvider, "Microsoft.Azure.IoTHub")
        {
            this.serviceProvider = serviceProvider;
            this.tenantService = (IAzureRMTenantService)serviceProvider.GetService(typeof(IAzureRMTenantService));
            this.NeedToAuthenticateText = Resource.NeedToAuthenticateText;
        }

        public override async Task<bool> SelectedAccountHasSubscriptions()
        {
            IEnumerable<IAzureRMSubscription> azureRMSubscriptions = await this.GetAzureRMSubscriptions();

            return azureRMSubscriptions.Any();
        }

        private async Task<IEnumerable<IAzureRMSubscription>> GetAzureRMSubscriptions()
        {
            IEnumerable<IAzureRMSubscription> subscriptions = Enumerable.Empty<IAzureRMSubscription>();

            Account account = await this.GetAccountAsync();
            if (account != null && !account.NeedsReauthentication)
            {
                IEnumerable<IAzureRMTenant> tenants = await this.tenantService.GetTenantsAsync(account);
                foreach (IAzureRMTenant tenant in tenants)
                {
                    subscriptions = subscriptions.Concat(await tenant.GetSubscriptionsAsync());
                }
            }

            return subscriptions;
        }

        public async Task<IEnumerable<IAzureIoTHub>> GetAzureIoTHubs(IAzureIoTHubAccountManager accountManager, CancellationToken cancellationToken)
        {
            IEnumerable<IAzureRMSubscription> subscriptions = await this.GetAzureRMSubscriptions().ConfigureAwait(false);
            List<IAzureIoTHub> iotHubAccounts = new List<IAzureIoTHub>();
            foreach (IAzureRMSubscription subscription in subscriptions)
            {
                IEnumerable<IAzureIoTHub> subscriptionAccounts = await accountManager.EnumerateIoTHubAccountsAsync(subscription, cancellationToken).ConfigureAwait(false);
                iotHubAccounts.AddRange(subscriptionAccounts);
            }

            return iotHubAccounts;
        }

        public async Task<IAzureIoTHub> CreateIoTHub(
            IAzureIoTHubAccountManager accountManager, CancellationToken cancellationToken)
        {
            Account account = await this.GetAccountAsync();
            Debug.Assert(account != null && !account.NeedsReauthentication);

            return await accountManager.CreateIoTHubAsync(this.serviceProvider, account, cancellationToken);
        }
    }
}
