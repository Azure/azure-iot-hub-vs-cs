using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureIoTHubConnectedService;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    internal sealed class AzureIoTHubAccountProviderGrid : ConnectedServiceGrid
    {
        private IServiceProvider serviceProvider;
        private IAzureIoTHubAccountManager storageAccountManager;
        private Authenticator authenticator;

        public AzureIoTHubAccountProviderGrid(IAzureIoTHubAccountManager storageAccountManager, IServiceProvider serviceProvider)
        {
            this.storageAccountManager = storageAccountManager;
            this.serviceProvider = serviceProvider;

            this.Description = Resource.IoTHubProvdierDescription;
            this.GridHeaderText = Resource.GridHeaderText;
            this.ServiceInstanceNameLabelText = Resource.ServiceInstanceNameLabelText;
            this.NoServiceInstancesText = Resource.NoServiceInstancesText;
            this.CreateServiceInstanceText = Resource.CreateServiceInstanceText;
        }

        private Authenticator Authenticator
        {
            get
            {
                if (this.authenticator == null)
                {
                    this.authenticator = new Authenticator(this.serviceProvider);
                    this.authenticator.PropertyChanged += this.OnAuthenticatorPropertyChanged;
                    this.authenticator.AuthenticationChanged += this.OnAuthenticatorAuthenticationChanged;
                    this.CalculateCanCreateServiceInstance();
                }
                return this.authenticator;
            }
        }

        public override async Task<IEnumerable<ConnectedServiceInstance>> EnumerateServiceInstancesAsync(CancellationToken ct)
        {
            //IEnumerable<IAzureStorageAccount> storageAccounts = await this.Authenticator.GetStorageAccounts(this.storageAccountManager, ct).ConfigureAwait(false);
            IEnumerable<IAzureStorageAccount> storageAccounts = await this.Authenticator.GetStorageAccounts(this.storageAccountManager, ct);
            ct.ThrowIfCancellationRequested();
            return storageAccounts.Select(p => AzureIoTHubAccountProviderGrid.CreateServiceInstance(p)).ToList();

            // throw new NotImplementedException();
        }

        private static ConnectedServiceInstance CreateServiceInstance(IAzureStorageAccount storageAccount)
        {
            ConnectedServiceInstance instance = new ConnectedServiceInstance();

            instance.InstanceId = storageAccount.Id;
            instance.Name = storageAccount.Properties["StorageAccount"];

            foreach (var property in storageAccount.Properties)
            {
                instance.Metadata.Add(property.Key, property.Value);
            }

            instance.Metadata.Add("StorageAccountName", storageAccount);

            return instance;
        }

        public override async Task<ConnectedServiceInstance> CreateServiceInstanceAsync(CancellationToken ct)
        {
            ConnectedServiceInstance result = null;
            IAzureStorageAccount createdAccount = await this.Authenticator.CreateStorageAccount(this.storageAccountManager, ct).ConfigureAwait(false);
            if (createdAccount != null)
            {
                result = AzureIoTHubAccountProviderGrid.CreateServiceInstance(createdAccount);
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this.authenticator != null)
                    {
                        this.authenticator.PropertyChanged -= this.OnAuthenticatorPropertyChanged;
                        this.authenticator.AuthenticationChanged -= this.OnAuthenticatorAuthenticationChanged;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnAuthenticatorAuthenticationChanged(object sender, AuthenticationChangedEventArgs e)
        {
            this.CalculateCanCreateServiceInstance();
        }

        private void OnAuthenticatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Authenticator.IsAuthenticated))
            {
                this.CalculateCanCreateServiceInstance();
            }
        }

        private async void CalculateCanCreateServiceInstance()
        {
            string noServicesText = "Resource1.StorageNoServiceInstancesText";
            this.CanCreateServiceInstance = this.Authenticator.IsAuthenticated;
            if (this.CanCreateServiceInstance)
            {
                this.CanCreateServiceInstance = (await this.Authenticator.SelectedAccountHasSubscriptions());
                if (!this.CanCreateServiceInstance)
                {
                    noServicesText = "Resource1.StorageNoSubscriptionsText";
                }
            }
            this.NoServiceInstancesText = noServicesText;
        }
    }
}