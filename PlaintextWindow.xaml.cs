using Dzik.domain;
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
    }
}
