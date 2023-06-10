using Dzik.domain;
using Dzik.Properties;
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

namespace Dzik
{
    /// <summary>
    /// Interaction logic for PlaintextWindow.xaml
    /// </summary>
    public partial class PlaintextWindow : Window
    {
        internal PlaintextWindow(Window owner, DecryptedMsg decryptedMsg)
        {
            this.Owner = owner;
            InitializeComponent();

            SourceInitialized += WindowRoot_SourceInitialized;
            Closing += WindowRoot_Closing;

            Output.Text = decryptedMsg.plainText;

            var daysPluralIfNeeded = "dni";
            if (decryptedMsg.daysSinceEncryption == 1) daysPluralIfNeeded = "dzień";
            EncryptionAgeLabel.Content = $"Zaszyfrowano około {decryptedMsg.daysSinceEncryption} {daysPluralIfNeeded} temu.";

            if(decryptedMsg.daysSinceEncryption > Constants.ReplayAttackMaxDaysWithoutWarning)
            {
                EncryptionAgeLabel.Foreground = Brushes.Red;               
            }
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        // Remembering window location
        private void WindowRoot_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Settings.Default.PlaintextWindowTop;
            this.Left = Settings.Default.PlaintextWindowLeft;
            this.Height = Settings.Default.PlaintextWindowHeight;
            this.Width = Settings.Default.PlaintextWindowWidth;
        }

        private void WindowRoot_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Settings.Default.PlaintextWindowTop = RestoreBounds.Top;
                Settings.Default.PlaintextWindowLeft = RestoreBounds.Left;
                Settings.Default.PlaintextWindowHeight = RestoreBounds.Height;
                Settings.Default.PlaintextWindowWidth = RestoreBounds.Width;
            }
            else
            {
                Settings.Default.PlaintextWindowTop = this.Top;
                Settings.Default.PlaintextWindowLeft = this.Left;
                Settings.Default.PlaintextWindowHeight = this.Height;
                Settings.Default.PlaintextWindowWidth = this.Width;
            }

            Settings.Default.Save();
        }

    }
}
