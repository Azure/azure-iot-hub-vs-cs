// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AzureIoTHubConnectedService;
using Microsoft.VisualStudio.ConnectedServices;

namespace AzureIoTHubConnectedService
{
    internal sealed class AzureIoTHubAccountProviderGrid : ConnectedServiceGrid
    {
        private IServiceProvider serviceProvider;
        private IAzureIoTHubAccountManager iotHubAccountManager;
        private Authenticator authenticator;

        public AzureIoTHubAccountProviderGrid(IAzureIoTHubAccountManager accountManager, IServiceProvider serviceProvider)
        {
            this.iotHubAccountManager = accountManager;
            this.serviceProvider = serviceProvider;

            this.Description = Resource.IoTHubProvdierDescription;
            this.GridHeaderText = Resource.GridHeaderText;
            this.ServiceInstanceNameLabelText = Resource.ServiceInstanceNameLabelText;
            this.NoServiceInstancesText = Resource.NoServiceInstancesText;
            this.CreateServiceInstanceText = Resource.CreateServiceInstanceText;
            this.ConfigureServiceInstanceText = "Select Devices";
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

        public override IEnumerable<Tuple<string, string>> ColumnMetadata
        {
            get
            {
                return new[]
                {
                    Tuple.Create("SubscriptionName", Resource.Subscription),
                    Tuple.Create("Region", Resource.Region),
                    Tuple.Create("ResourceGroup", Resource.ResourceGroup),
                    Tuple.Create("Tier", Resource.Tier),
                };
            }
        }

        public override Task<bool> ConfigureServiceInstanceAsync(ConnectedServiceInstance instance, CancellationToken ct)
        {
            return base.ConfigureServiceInstanceAsync(instance, ct);
        }

        public override Task<ConnectedServiceAuthenticator> CreateAuthenticatorAsync()
        {
            return Task.FromResult<ConnectedServiceAuthenticator>(this.Authenticator);
        }

        public override async Task<IEnumerable<ConnectedServiceInstance>> EnumerateServiceInstancesAsync(CancellationToken ct)
        {
            IEnumerable<IAzureIoTHub> hubs = await this.Authenticator.GetAzureIoTHubs(this.iotHubAccountManager, ct).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            return hubs.Select(p => AzureIoTHubAccountProviderGrid.CreateServiceInstance(p)).ToList();
        }

        private static ConnectedServiceInstance CreateServiceInstance(IAzureIoTHub iotHubAccount)
        {
            ConnectedServiceInstance instance = new ConnectedServiceInstance();

            instance.InstanceId = iotHubAccount.Id;
            instance.Name = iotHubAccount.Properties["IoTHubName"];

            foreach (var property in iotHubAccount.Properties)
            {
                instance.Metadata.Add(property.Key, property.Value);
            }

            instance.Metadata.Add("IoTHubAccount", iotHubAccount);
            instance.Metadata.Add("Cancel", false);
            instance.Metadata.Add("TPM", false);

            return instance;
        }

        public override async Task<ConnectedServiceInstance> CreateServiceInstanceAsync(CancellationToken ct)
        {
            ConnectedServiceInstance result = null;
            IAzureIoTHub createdAccount = await this.Authenticator.CreateIoTHub(this.iotHubAccountManager, ct).ConfigureAwait(false);
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

        private void CalculateCanCreateServiceInstance()
        {
            this.CanCreateServiceInstance = false;
// New IoT Hub instance creation is not yet supported, so disable the link unconditionally
#if false
            string noServicesText = Resource.NoServiceInstancesText;
            this.CanCreateServiceInstance = this.Authenticator.IsAuthenticated;
            if (this.CanCreateServiceInstance)
            {
                this.CanCreateServiceInstance = (await this.Authenticator.SelectedAccountHasSubscriptions());
                if (!this.CanCreateServiceInstance)
                {
                    noServicesText = Resource.NoServiceInstancesText;
                }
            }
            this.NoServiceInstancesText = noServicesText;
#endif
        }
    }
}