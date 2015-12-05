using Microsoft.VisualStudio.Services.Client.AccountManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Interaction logic for AccountPicker.xaml
    /// </summary>
    internal partial class AccountPicker : UserControl, IDisposable
    {
        private IWpfAccountPicker picker;
        private AccountPickerViewModel viewModel;
        private bool isRespondingToSelectedAccountPropertyChanged;

        public AccountPicker(AccountPickerViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.viewModel.AuthenticationChanged += this.ViewModel_AuthenticationChanged;

            this.InitializeComponent();
        }

        public void Dispose()
        {
            if (this.picker != null)
            {
                this.picker.PropertyChanged -= this.Picker_PropertyChanged;
                this.picker.Dispose();
            }

            if (this.viewModel != null)
            {
                this.viewModel.AuthenticationChanged -= this.ViewModel_AuthenticationChanged;
            }
        }

        private async void AccountPickerHost_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.picker == null)
            {
                IVsAccountManagementService accountManagementService = Package.GetGlobalService(typeof(SVsAccountManagementService)) as IVsAccountManagementService;
                if (accountManagementService == null)
                {
                    Debug.Fail("Could not retrieve an IVsAccountManagementService.");
                    return;
                }

                AccountPickerOptions accountPickerOptions = new AccountPickerOptions(
                    Window.GetWindow(this),
                    this.viewModel.HostId);
                this.picker = await accountManagementService.CreateWpfAccountPickerAsync(accountPickerOptions);
                this.picker.SelectedAccount = await this.viewModel.GetAccountAsync();

                this.picker.PropertyChanged += this.Picker_PropertyChanged;
                this.AccountPickerHost.Content = this.picker.Control;
            }
        }

        private async void Picker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // The property changed event can get raised on the non-UI thread.  This causes issues
            // from logic expecting the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (e.PropertyName == nameof(IWpfAccountPicker.SelectedAccount))
            {
                try
                {
                    this.isRespondingToSelectedAccountPropertyChanged = true;
                    await this.viewModel.SetAccountAsync((Account)this.picker.SelectedAccount);
                }
                finally
                {
                    this.isRespondingToSelectedAccountPropertyChanged = false;
                }
            }
            else if (e.PropertyName == nameof(IWpfAccountPicker.SelectedAccountAuthenticationState))
            {
                this.viewModel.IsAuthenticated =
                    this.picker.SelectedAccountAuthenticationState == AuthenticationState.Authenticated;
            }
        }

        private async void ViewModel_AuthenticationChanged(object sender, EventArgs e)
        {
            // whenever the AuthenticationChanged event is raised outside of the AccountPicker changing accounts,
            // ensure the AccountPicker is synced with the current account
            if (!this.isRespondingToSelectedAccountPropertyChanged)
            {
                Account account = await this.viewModel.GetAccountAsync();
                this.picker.SelectedAccount = account;
            }
        }
    }
}
