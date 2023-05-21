using Dzik.common;
using Dzik.crypto.protocols;
using Dzik.crypto.utils;
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
    /// Interaction logic for ReceiveResponseWindow.xaml
    /// </summary>
    public partial class ReceiveResponseWindow : Window
    {
        private string responseWithMarker;
        private Action<KeysVault> onKeysReceivedInExchange;

        public ReceiveResponseWindow(string responseWithMarker, Action<KeysVault> onKeysReceivedInExchange)
        {
            this.responseWithMarker = responseWithMarker;
            this.onKeysReceivedInExchange = onKeysReceivedInExchange;

            InitializeComponent();
        }

        private void FinalizeButton_Click(object sender, RoutedEventArgs e)
        {
            // input validation:
            var KekHex = PrivKeyKEKInput.Text;
            byte[] kek;
            if (KekHex.Length == 0)
            {
                DialogShower.ShowError("Wpisz KEK klucza prywatnego");
                return;
            }

            try
            {
                kek = ByteArrayHexStringConverters.ToByteArray(KekHex);
            }catch
            {
                DialogShower.ShowError("Wpisz poprawny KEK klucza prywatnego");
                return;
            }

            // with password:
            if (passwdInput1.Text.Length > 0 || passwdInput2.Text.Length > 0)
            {
                if(passwdInput1.Text != passwdInput2.Text)
                {
                    DialogShower.ShowError("Wpisane hasła się różnią");
                    return;
                }

                DzikKeyAgreement.AcceptResponse(responseWithMarker, kek, passwdInput1.Text, onKeysReceivedInExchange);
                return;
            }

            // password-less:
            DzikKeyAgreement.AcceptResponse(responseWithMarker, kek, null, onKeysReceivedInExchange);
        }
    }
}
