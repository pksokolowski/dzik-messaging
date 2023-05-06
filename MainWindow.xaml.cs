using Dzik.common;
using Dzik.crypto.algorithms;
using Dzik.crypto.api;
using Dzik.crypto.protocols;
using Dzik.data;
using Dzik.domain;
using Dzik.editing;
using Dzik.Properties;
using Dzik.replying;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dzik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KeysVault keysVault;
        private MsgCryptoTool msgCryptoTool;

        public MainWindow()
        {
            InitializeComponent();
            LoadMasterKeys();

            EditorStartBehavior.InitializeEditor(Input);
            DataLossPreventor.Setup(this, Input);

            SourceInitialized += MainWindow_SourceInitialized;
            Closing += MainWindow_Closing;
        }

        private void LoadMasterKeys()
        {
            if (keysVault != null) return;

            var keys = StorageManager.ReadMasterKeys();
            using (keys)
            {
                if (keys == null)
                {
                    DialogShower.ShowInfo("Nie znaleziono kluczy, stworzone zostaną nowe");
                    var newKeys = MasterKeysGenerator.GenerateMasterKeys();
                    var successfullySavedNewKeys = StorageManager.WriteMasterKeys(newKeys);


                    var keysFromStorage = StorageManager.ReadMasterKeys();

                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(keysFromStorage.bytes, newKeys))
                    {
                        throw new Exception("Saved new keys are not the same as the keys read right after!");
                    }

                    keysFromStorage.Dispose();

                    keysVault = MasterKeysPacker.UnpackKeys(newKeys);
                    msgCryptoTool = new MsgCryptoTool(keysVault);
                }
                else
                {
                    keysVault = MasterKeysPacker.UnpackKeys(keys);
                    msgCryptoTool = new MsgCryptoTool(keysVault);
                }
            }
        }

        private void QuoteButton_Click(object sender, RoutedEventArgs e)
        {
            var initialSelectionStart = Input.SelectionStart;
            var clipboardText = Clipboard.GetText();

            // Handlers in order, only one can handle the content, then return.
            if (CiphertextHandler.Handle(this, clipboardText, msgCryptoTool)) return;
            QuotationHandler.Handle(Input, clipboardText);
        }

        private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DraftStorage.Store(Input);
            }
            catch (Exception)
            {
                DialogShower.ShowError("Zapisywanie wersji roboczej nie powiodło się.\n\nZalecane jest zbackupowanie tekstu ręcznie, just in case.");
            }
        }

        private void MarkImgButton_Click(object sender, RoutedEventArgs e)
        {
            var initialSelectionStart = Input.SelectionStart;

            var imgMarkerText = Constants.MARKER_IMAGE_TAG;
            // add newline before, if missing
            if (!LinesPrepender.IsCarretAtTheBeginningOfLine(Input)) imgMarkerText = '\n' + imgMarkerText;

            var imgMarkerTextNoteInitialText = " notka orientacyjna o jakie zdjęcie chodzi";
            var initialImgText = LinesPrepender.Prepended(imgMarkerTextNoteInitialText, imgMarkerText, true);

            ContentPaster.PasteInto(Input, initialImgText);
            Input.SelectionStart = initialSelectionStart + imgMarkerText.Length + 1;
            Input.SelectionLength = imgMarkerTextNoteInitialText.Length - 1;
            Input.Focus();
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            // save draft before.
            try
            {
                DraftStorage.Store(Input);
            }
            catch (Exception)
            {
                DialogShower.ShowError("Zapisywanie wersji roboczej nie powiodło się. Nie zatrzymano jednak procesu generowania odpowiedzi.");
            }

            try
            {
                var msgParts = ReplyAssembler.Assemble(Input.Text, msgCryptoTool);

                var replyWindow = new ReplyWindow(msgParts);
                this.IsEnabled = false;
                replyWindow.Closed += (_, __) => { this.IsEnabled = true; };
                replyWindow.Show();
            }
            catch (Exception)
            {
                DialogShower.ShowError("Generowanie odpowiedzi nie powiodło się. Możliwe powody:\n\n- bardzo długi blok tekstu, który samodzielnie przekracza limit długości pojedynczej wiadomości.\n\nBłąd szyfrowania, jeżeli występują elementy oznaczone do zaszyfrowania.");
            }

        }


        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Settings.Default.Top;
            this.Left = Settings.Default.Left;
            this.Height = Settings.Default.Height;
            this.Width = Settings.Default.Width;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Settings.Default.Top = RestoreBounds.Top;
                Settings.Default.Left = RestoreBounds.Left;
                Settings.Default.Height = RestoreBounds.Height;
                Settings.Default.Width = RestoreBounds.Width;
            }
            else
            {
                Settings.Default.Top = this.Top;
                Settings.Default.Left = this.Left;
                Settings.Default.Height = this.Height;
                Settings.Default.Width = this.Width;
            }

            Settings.Default.Save();
        }
    }
}
