// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace AzureIoTHubConnectedService
{
    public class AccountPickerViewModel : IDisposable, INotifyPropertyChanged
    {
        private AccountKey accountKey;
        private bool isAuthenticated;
        private bool accountInitialized;
        private IAccountManager accountManager;
        private IAzureAuthenticationManager authenticationManager;
        private string hostId;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler AuthenticationChanged;

        public AccountPickerViewModel(IServiceProvider serviceProvider, string hostId)
        {
            this.hostId = hostId;

            this.authenticationManager = (IAzureAuthenticationManager)serviceProvider.GetService(typeof(IAzureAuthenticationManager));
            this.authenticationManager.SubscriptionsChanged += this.OnSubscriptionsChanged;

            this.accountManager = (IAccountManager)serviceProvider.GetService(typeof(SVsAccountManager));
        }

        public IAzureAuthenticationManager AuthenticationManager
        {
            get { return this.authenticationManager; }
        }

        public bool IsAuthenticated
        {
            get { return this.isAuthenticated; }
            set
            {
                if (this.isAuthenticated != value)
                {
                    this.isAuthenticated = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string HostId
        {
            get { return this.hostId; }
        }

        public void Dispose()
        {
            this.authenticationManager.SubscriptionsChanged -= this.OnSubscriptionsChanged;
        }

        public async Task<Account> GetAccountAsync()
        {
            Account account;
            if (!this.accountInitialized)
            {
                account = await this.authenticationManager.GetCurrentVSAccountAsync();
                await this.SetAccountAsync(account);
            }
            else
            {
                // The account is an immutable object, therefore it needs to be retrieved from the store each
                // time in order to get the most recent state (e.g. NeedsReauthentication).
                account = this.accountManager.Store.GetAllAccounts()
                    .FirstOrDefault(a => AccountKey.KeyComparer.Equals(this.accountKey, a));
            }

            return account;
        }

        public async Task SetAccountAsync(Account value)
        {
            this.accountInitialized = true;

            if (!AccountKey.KeyComparer.Equals(this.accountKey, value))
            {
                this.accountKey = value;
                this.IsAuthenticated = value != null && !value.NeedsReauthentication;

                await this.authenticationManager.SetCurrentVSAccountAsync(value);
                this.OnAuthenticationChanged();
            }
        }

        private async void OnSubscriptionsChanged(object sender, EventArgs e)
        {
            // Bug 1136383 - When a new account is added via the AccountPicker control the AuthenticationChanged
            // event will get raised.  This triggers core connected services to invoke the grid's EnumerateServiceInstancesAsync
            // method which enumerates the subscriptions.  The issue is that sometimes CAT hasn't responded
            // the new account being added and created a mirror of the account in the IAzureAuthenticationManager.UserAccounts
            // collection.  Because of this no subscriptions will be returned for the new account.  To work around this
            // issue the AuthenticationChanged event is also raised as a result of AzureSubscriptionsChanged so
            // that core will call the grid's EnumerateServiceInstancesAsync.

            // ensure we are in sync with the current VS Account
            this.accountInitialized = false;
            Account currentVSAccount = await this.GetAccountAsync();
            Debug.Assert(AccountKey.KeyComparer.Equals(this.accountKey, currentVSAccount), "Calling GetAccountAsync with accountInitialized should have set this.accountKey");

            this.OnAuthenticationChanged();
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnAuthenticationChanged()
        {
            this.AuthenticationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
