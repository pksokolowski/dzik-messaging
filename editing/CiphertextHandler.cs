﻿using Dzik.common;
using Dzik.crypto.utils;
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
                if (!TagUtil.StartsWithTag(Constants.MARKER_TO_DECRYPT_TAG, trimmedContent)) return false;

                var ciphertext = trimmedContent.Substring(Constants.MARKER_TO_DECRYPT_TAG.Length);
                var decryptedMsg = decryptor.Decrypt(ciphertext);

                PlaintextWindow plainWindow = new PlaintextWindow(window, decryptedMsg);
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
