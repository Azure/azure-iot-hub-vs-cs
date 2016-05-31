using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Interaction logic for TPMSelector.xaml
    /// </summary>
    internal partial class TPMSelector : DialogWindow
    {
        internal TPMSelector()
        {
            InitializeComponent();
            this.rbUseTPM.IsChecked = false;
            this.rbDoNotTPM.IsChecked = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start("http://aka.ms/tpmiothubcs");
        }
    }
}
