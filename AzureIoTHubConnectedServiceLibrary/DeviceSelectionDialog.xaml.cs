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
        Func<string, Task<Device>> createNewDevice;

        public DeviceSelectionDialog(Task<IEnumerable<Device>> devices, Func<string, Task<Device>> newDeviceCreator)
        {
            this.createNewDevice = newDeviceCreator;
            this.Devices = new List<Device>();
            InitializeComponent();
#pragma warning disable CS4014
            this.listBox.Items.Add(Resource.LoadingDevices);
            PopulateDialog(devices);
        }

        private async Task PopulateDialog(Task<IEnumerable<Device>> devices)
        {
            this.Devices = (await devices).ToList();
            this.listBox.Items.Clear();
            foreach (var device in this.Devices)
            {
                this.listBox.Items.Add(device.Id);
            }

            if (this.listBox.Items.Count == 0)
            {
                this.okButton.IsEnabled = false;
            }
            else {
                this.okButton.IsEnabled = true;
                this.listBox.SelectedIndex = 0;
            }
        }

        public string SelectedDeviceID { get; set; }

        public List<Device> Devices { get; set; }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                this.SelectedDeviceID = this.listBox.SelectedItem.ToString();
                this.DialogResult = true;
            }
            else
            {
                this.DialogResult = false;
            }
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
                var newDevice = await this.createNewDevice(deviceId);
                if (newDevice != null)
                {
                    this.listBox.Items.Add(deviceId);
                    this.okButton.IsEnabled = true;
                    this.Devices.Add(newDevice);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
