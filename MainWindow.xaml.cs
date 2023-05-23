﻿using Dzik.common;
using Dzik.crypto.api;
using Dzik.crypto.protocols;
using Dzik.data;
using Dzik.editing;
using Dzik.keyStorageWindows;
using Dzik.Properties;
using Dzik.replying;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dzik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KeysVault keysVault;
        private MsgCryptoTool msgCryptoTool;

        private FileDropHandler fileDropHandler;

        public MainWindow()
        {
            InitializeComponent();

            DataLossPreventor.Setup(
                this,
                Input,
            hasUnsavedChanges =>
            {
                if (hasUnsavedChanges) { Title = "*Dzik"; } else { Title = "Dzik"; }
            },

            () => keysVault);

            fileDropHandler = new FileDropHandler(Input, () => { return keysVault; });

            SourceInitialized += MainWindow_SourceInitialized;
            Closing += MainWindow_Closing;
        }

        private void LoadMasterKeys()
        {
            if (keysVault != null) return;

            var keysStatus = StorageManager.GetMasterKeysState();

            switch (keysStatus)
            {
                case StorageManager.MasterKeysState.UNPROTECTED:
                    var keys = StorageManager.ReadMasterKeys();
                    using (keys)
                    {
                        AcceptKeysVault(MasterKeysPacker.UnpackKeys(keys));
                    }
                    RestoreDraft();
                    break;

                case StorageManager.MasterKeysState.PASSWD_PROTECTED:
                    this.IsEnabled = false;
                    var passwdWindow = new PasswordWindow(this, (passwd, paswdWindow) =>
                    {
                        var keysCandidate = StorageManager.ReadPasswordProtectedMasterKeys(passwd);
                        if (keysCandidate == null)
                        {
                            // password is incorrect 1 (or error loading)
                            paswdWindow.IndicateWrongPassword();
                            return;
                        }
                        using (keysCandidate)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                var vaultCandidate = MasterKeysPacker.UnpackKeys(keysCandidate);

                                var test = new byte[] { 1, 2, 3, 4, 5 };
                                var testEncrypted = vaultCandidate.EncryptAndSign(test);
                                var roundTripResult = vaultCandidate.VerifyAndDecrypt(testEncrypted);
                                if (roundTripResult == null || !Enumerable.SequenceEqual(test, roundTripResult.plainText))
                                {
                                    // password is incorrect 2
                                    paswdWindow.IndicateWrongPassword();
                                }
                                else
                                {
                                    AcceptKeysVault(vaultCandidate);
                                    RestoreDraft();
                                    paswdWindow.Close();
                                    this.IsEnabled = true;
                                }
                            }));
                        }
                    });
                    passwdWindow.Show();
                    passwdWindow.Focus();

                    break;

                case StorageManager.MasterKeysState.NOT_PRESENT:
                    DzikKeyAgreement.Initialize(
                        () =>
                        {
                            // on disabling main window needed
                            Dispatcher.Invoke(new Action(() =>
                            {
                                this.IsEnabled = false;
                            }));
                        },
                        vault =>
                        {
                            // on new keys generated
                            AcceptKeysVault(vault);
                            Dispatcher.Invoke(new Action(() =>
                            {
                                this.IsEnabled = true;
                            }));
                        },
                        () =>
                        {
                            // on key exchange response ready
                            Dispatcher.Invoke(new Action(() =>
                               {
                                   ContentPaster.PasteAtTheBeginning(Input, "Poniższa linijka zostanie podmieniona na wiadomość konfiguracyjną. Zachowaj ją w pierwszej wiadomości :)\n" + Constants.MARKER_INSERT_KEY_EXCHANGE_RESPONSE_HERE + "\n\n");
                                   DraftStorage.Store(Input, keysVault);
                                   Input.Select(Input.Text.Length - 1, 0);
                                   this.IsEnabled = true;
                               }));
                        });
                    RestoreDraft();
                    break;
            }
        }

        private void QuoteButton_Click(object sender, RoutedEventArgs e)
        {
            var initialSelectionStart = Input.SelectionStart;
            var clipboardText = Clipboard.GetText();

            // Handlers in order, only one can handle the content, then return.
            if (CiphertextHandler.Handle(this, clipboardText, msgCryptoTool)) return;
            if (KeyAgreementResponseHandler.Handle(clipboardText, keysVault, vault => { AcceptKeysVault(vault); })) return;
            QuotationHandler.Handle(Input, clipboardText);
        }

        private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDraft();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveDraft();
                e.Handled = true;
            }
        }

        private void SaveDraft()
        {
            try
            {
                DraftStorage.Store(Input, keysVault);
            }
            catch
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

        private void EncryptSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            PrefixAdder.Handle(Input, Constants.MARKER_TO_ENCRYPT_TAG);
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            // save draft before.
            try
            {
                DraftStorage.Store(Input, keysVault);
            }
            catch
            {
                DialogShower.ShowError("Zapisywanie wersji roboczej nie powiodło się. Nie zatrzymano jednak procesu generowania odpowiedzi.");
            }

            try
            {
                var msgParts = ReplyAssembler.Assemble(Input.Text, msgCryptoTool, new KeyAgreementResponseProviderImpl());

                var replyWindow = new ReplyWindow(msgParts);
                this.IsEnabled = false;
                replyWindow.Closed += (_, __) => { this.IsEnabled = true; };
                replyWindow.Show();
            }
            catch
            {
                DialogShower.ShowError("Generowanie odpowiedzi nie powiodło się. Prawdopodobne powody:\n\n- bardzo długi blok tekstu, który samodzielnie przekracza limit długości pojedynczej wiadomości.");
            }

        }

        internal void RestoreDraft()
        {
            var draft = DraftStorage.ReadDraft(keysVault);
            if (draft == null) return;

            Input.Text = draft;
            DataLossPreventor.OnDataSaved();
        }

        private void AcceptKeysVault(KeysVault vault)
        {
            keysVault = vault;
            msgCryptoTool = new MsgCryptoTool(vault);
        }


        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Settings.Default.Top;
            this.Left = Settings.Default.Left;
            this.Height = Settings.Default.Height;
            this.Width = Settings.Default.Width;

            LoadMasterKeys();
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
