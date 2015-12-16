using System.Windows;
using System;
using Microsoft.VisualStudio.ConnectedServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NuGet.VisualStudio;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Devices;
using System.Globalization;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Interaction logic for MyDialog.xaml
    /// </summary>
    internal partial class DeviceSelectionDialog : DialogWindow, IDisposable
    {
        Func<string, Task> newDeviceCreator;

        public DeviceSelectionDialog(Task<IEnumerable<Device>> devices, Func<string, Task> newDeviceCreator)
        {
            this.newDeviceCreator = newDeviceCreator;
            InitializeComponent();
#pragma warning disable CS4014
            PopulateDialog(devices);
        }

        private async Task PopulateDialog(Task<IEnumerable<Device>> devices)
        {
            foreach (var device in await devices)
            {
                this.listBox.Items.Add(device.Id);
            }
        }

        public string SelectedDevice { get; set; }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedDevice = this.listBox.SelectedItem.ToString();
            this.DialogResult = true;
            this.Close();
        }
        private async void newButton_Click(object sender, RoutedEventArgs e)
        {
            var newDeviceDlg = new NewDevice();
            var action = newDeviceDlg.ShowDialog();
            if (action.HasValue && action.Value)
            {
                // Create a new device and add it to the list
                var deviceId = newDeviceDlg.textBox.Text;
                this.listBox.Items.Add(deviceId);
                await this.newDeviceCreator(deviceId);
            }
        }

        public void Dispose()
        {
        }
    }
}
