using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MyLife
{
    /// <summary>
    /// Interaction logic for FirstTimeWindow.xaml
    /// </summary>
    public partial class FirstTimeWindow : Window
    {
        private Config config;
        private MainWindow win;

        public FirstTimeWindow(Config config, MainWindow win)
        {
            this.config = config;
            this.win = win;
            InitializeComponent();
            Show();
        }

        private void finishBtn_Click(object sender, RoutedEventArgs e)
        {
            if(IsValid())
            {
                if(config != null)
                    config.ClientId = clientIdTxt.Text;
                win.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Check if you enter the client id correctly.", "Client ID is not valid!");
            }
        }

        private bool IsValid()
        {
            if(clientIdTxt.Text.Length == 18)
            {
                foreach(var c in clientIdTxt.Text)
                {
                    if(c < 0x0030 || c > 0x0039)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private void clientIdTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Back || e.Key == Key.Delete)
            {
                return;
            }
            e.Handled = clientIdTxt.Text.Length >= 18;
        }

        private void clientIdTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            finishBtn.IsEnabled = IsValid();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(win.Visibility == Visibility.Hidden)
            {
                win.Close();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.OriginalString));
            e.Handled = true;
        }
    }
}
