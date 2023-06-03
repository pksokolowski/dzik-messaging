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

        private void passwdInput1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (passwdInput1.Text.Length == 0)
            {
                PassStrengthEstimateLabel.Content = "";
                return;
            }

            var estimate = PasswordStrengthGauge.Gauge(passwdInput1.Text);
            var tip = "";
            var estimateColor = Brushes.Red;
            switch (estimate)
            {
                case PasswordStrengthGauge.PasswordStrength.WEAK: tip = "(0/5) Słabe w opór"; estimateColor = Brushes.Red; break;
                case PasswordStrengthGauge.PasswordStrength.CASUAL: tip = "(1/5) Może starczy na ciekawskich"; estimateColor = Brushes.Orange; break;
                case PasswordStrengthGauge.PasswordStrength.SEMI_SERIOUS: tip = "(2/5) Starczy na ciekawskich laików"; estimateColor = Brushes.DarkKhaki; break;
                case PasswordStrengthGauge.PasswordStrength.ENTREPRISE: tip = "(3/5) Bezpieczne!"; estimateColor = Brushes.Green; break;
                case PasswordStrengthGauge.PasswordStrength.TOP_SECRET: tip = "(4/5) Bardzo bezpieczne!"; estimateColor = Brushes.DarkGreen; break;
                case PasswordStrengthGauge.PasswordStrength.MAXED_OUT: tip = "(5/5) Golden"; estimateColor = Brushes.Gold; break;
            }

            PassStrengthEstimateLabel.Content = tip;
            PassStrengthEstimateLabel.Foreground = estimateColor;
        }
    }
}
