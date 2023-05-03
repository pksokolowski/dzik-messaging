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
            if (!content.StartsWith(Constants.MARKER_TO_DECRYPT_TAG)) return false;

            var ciphertext = content.Substring(Constants.MARKER_TO_DECRYPT_TAG.Length);
            var plainText = decryptor.Decrypt(ciphertext);

            PlaintextWindow plainWindow = new PlaintextWindow(window, plainText);
            plainWindow.Show();

            return true;
        }
    }
}
