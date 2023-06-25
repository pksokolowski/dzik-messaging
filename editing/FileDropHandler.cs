using Dzik.common;
using Dzik.crypto.protocols;
using Dzik.letter;
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
        private Window window;

        public FileDropHandler(Window window, TextBox dropSurface, Func<KeysVault> getKeysVault)
        {
            this.getKeysVault = getKeysVault;
            this.window = window;
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
                var loadingIndicator = new LoadingIndicator(window, LoadingIndicatorLocation.CenterOwner, false);

                foreach (string file in files)
                {
                    var result = await Task.Run(() => FileEncryptionTool.HandleFile(file, keysVault, onSpecialFileTypeDropped));
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
                            loadingIndicator.CloseIndicator();
                            return;                            
                    }

                    var fileName = Path.GetFileName(file);
                    builder.AppendLine($"Plik: \"{fileName}\" - {resultDescription}\n");
                }

                loadingIndicator.CloseIndicator();                

                builder.AppendLine("Szyfrowanie plików operuje na kopiach - plik zaszyfrowany nazywa się tak jak oryginał, ale nie ma rozszerzenia.");
                DialogShower.ShowInfo(builder.ToString());
            }
        }

        private void onSpecialFileTypeDropped(SpecialFileType type, byte[] fileBytes, long daysSinceEncryption)
        {
            switch (type)
            {
                case SpecialFileType.XamlMessage:
                    if (daysSinceEncryption > Constants.ReplayAttackMaxDaysWithoutWarning)
                    {
                        ShowReplayAttackWarning(daysSinceEncryption);
                    }
                    ShowDebateWindow(fileBytes);
                    break;
                default:
                    DialogShower.ShowError("Nieznane rozszerzenie specjalne. Czy używasz tej samej wersji La Vache co Twój rozmówca?");
                    break;
            }
        }

        private void ShowReplayAttackWarning(long daysSinceEncryption)
        {
            DialogShower.ShowDangerousWarning("Możliwy replay attack", $"Szyfrogram jest autentyczny, ale szyfrowanie odbyło się ~{daysSinceEncryption} dni temu.\n\nJeżeli wiadomość przyszła jako 'nowa', może to być próba wykorzystania przez osoby trzecie znanego, poprawnego szyfrogramu z przeszłości, którego sensu adwersarz domyślił się np. po zachowaniach stron komunikacji.\n\nJeżeli świadomie deszyfrujesz wiadomość z odleglejszej przeszłości, możesz zignorować to ostrzeżenie.");
        }

        private void ShowDebateWindow(byte[] inboundMessageDecryptedBytes = null)
        {
            var keysVault = getKeysVault();
            if (keysVault == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var debateWindow = new DebateWindow(keysVault, inboundMessageDecryptedBytes);
                debateWindow.Show();
            });
        }
    }
}
