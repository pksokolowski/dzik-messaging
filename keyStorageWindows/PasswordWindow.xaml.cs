using Dzik.common;
using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
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

namespace Dzik.keyStorageWindows
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        private Action<SecureString, PasswordWindow> onPasswordEntered;
        public PasswordWindow(Window owner, Action<SecureString, PasswordWindow> onPasswdEntered)
        {
            this.Owner = owner;
            this.onPasswordEntered = onPasswdEntered;
            InitializeComponent();

            this.ContentRendered += PasswordWindow_ContentRendered;
        }

        private void PasswordWindow_ContentRendered(object sender, EventArgs e)
        {
            this.Activate();
            PasswordInput.Focus();
        }

        private async void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            await TryPassword();
        }

        private void CloseDzikButton_Click(object sender, RoutedEventArgs e)
        {
            Owner.Close();
            this.Close();
        }

        internal void IndicateWrongPassword()
        {
            Dispatcher.Invoke(new Action(() => { 
                MainBorder.BorderBrush = Brushes.Red; 
                PasswordInput.Clear();
                PasswordInput.Focus();
            }));
        }

        private async void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { MainBorder.BorderBrush = Brushes.LightGray; }));
            if (e.Key == Key.Enter)
            {
                await TryPassword();
            }
        }

        private async Task TryPassword()
        {
            Progress.Visibility = Visibility.Visible;
            await Task.Run(() => { onPasswordEntered(PasswordInput.SecurePassword, this); });
            Progress.Visibility = Visibility.Collapsed;
        }
    }
}
