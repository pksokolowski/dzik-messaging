using Dzik.common;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal class CiphertextHandler
    {
        internal static bool Handle(Window window, string content, Decryptor decryptor)
        {
            try
            {
                var trimmedContent = content.Trim();
                if (!trimmedContent.StartsWith(Constants.MARKER_TO_DECRYPT_TAG)) return false;

                var ciphertext = trimmedContent.Substring(Constants.MARKER_TO_DECRYPT_TAG.Length);
                var plainText = decryptor.Decrypt(ciphertext);

                PlaintextWindow plainWindow = new PlaintextWindow(window, plainText);
                plainWindow.Show();

                return true;
            }
            catch (Exception)
            {
                DialogShower.ShowError("Deszyfrowanie nie powiodło się.\n\nUpewnij się, że kopiujesz tylko tekst zaszyfrowany i nic poza nim. Możesz spróbować dwukrotnego kliku na szyfrogram - powinien zaznaczyć się wówczas w całości.");
                return true;
            }
        }
    }
}
