﻿using Dzik.common;
using Dzik.crypto.protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal class FileDropHandler
    {
        Func<KeysVault> getKeysVault;

        public FileDropHandler(TextBox dropSurface, Func<KeysVault> getKeysVault)
        {
            this.getKeysVault = getKeysVault;
            dropSurface.Drop += Drop;
            dropSurface.PreviewDragOver += PreviewDragOver;
        }

        private void PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private async void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var keysVault = getKeysVault();
                if (keysVault == null)
                {
                    return;
                }

                var builder = new StringBuilder();

                foreach (string file in files)
                {
                    var result = await Task.Run(() => FileEncryptionTool.HandleFile(file, keysVault));
                    var resultDescription = "";
                    switch (result)
                    {
                        case FileCryptoOperationResult.encrypted:
                            resultDescription = "ZASZYFROWANO";
                            break;
                        case FileCryptoOperationResult.decrypted:
                            resultDescription = "ODSZYFROWANO";
                            break;
                        case FileCryptoOperationResult.decryptedOldCiphertext:
                            resultDescription = $"ODSZYFROWANO (!)\nUwaga: powyższy szyfrogram powstał ponad {Constants.ReplayAttackMaxDaysWithoutWarning} dni temu.";
                            break;
                        case FileCryptoOperationResult.decryptionError:
                            resultDescription = "BŁĄD DESZYFROWANIA!";
                            break;
                        case FileCryptoOperationResult.fileIsEmpty:
                            resultDescription = "PLIK JEST PUSTY";
                            break;
                        case FileCryptoOperationResult.unknownError:
                            resultDescription = "BŁĄD";
                            break;
                        case FileCryptoOperationResult.XamlMessageDetected:
                            resultDescription = "ZNALEZIONO LIST";
                            break;
                    }

                    var fileName = Path.GetFileName(file);
                    builder.AppendLine($"Plik: \"{fileName}\" - {resultDescription}");
                }

                DialogShower.ShowInfo(builder.ToString());
            }
        }
    }
}
