using Dzik.common;
using Dzik.crypto.protocols;
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

namespace Dzik.keyStorageWindows
{
    /// <summary>
    /// Interaction logic for GenerateResponseWindow.xaml
    /// </summary>
    public partial class GenerateResponseWindow : Window
    {
        private Action<KeysVault> onNewKeysGenerated;
        private Action onKeyExchangeResponseReady;
        private byte[] challenge;

        public GenerateResponseWindow(Action<KeysVault> onNewKeysGenerated, Action onKeyExchangeResponseReady, byte[] challenge)
        {
            this.onNewKeysGenerated = onNewKeysGenerated;
            this.onKeyExchangeResponseReady = onKeyExchangeResponseReady;
            this.challenge = challenge;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // with password:
            if (passwdInput1.Text.Length > 0 || passwdInput2.Text.Length > 0)
            {
                if (passwdInput1.Text != passwdInput2.Text)
                {
                    DialogShower.ShowError("Wpisane hasła się różnią");
                    return;
                }

                DzikKeyAgreement.GenerateResponse(onNewKeysGenerated, onKeyExchangeResponseReady, challenge, passwdInput1.Text);
            }
            else
            {
                // without password:
                DzikKeyAgreement.GenerateResponse(onNewKeysGenerated, onKeyExchangeResponseReady, challenge, null);
            }

            Close();
        }
    }
}
