using Dzik.common;
using Dzik.crypto.api;
using Dzik.crypto.protocols;
using Dzik.data;
using Dzik.editing;
using Dzik.keyStorageWindows;
using Dzik.letter;
using Dzik.Properties;
using Dzik.replying;
using System;
using System.Linq;
using System.Windows;
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

        private ResourceDictionary DarkTheme;
        private ResourceDictionary LightTheme;

        public MainWindow()
        {
            InitializeComponent();

            DarkTheme = new ResourceDictionary();
            LightTheme = new ResourceDictionary();
            DarkTheme.Source = new Uri("Theme/DarkTheme.xaml", UriKind.Relative);
            LightTheme.Source = new Uri("Theme/LightTheme.xaml", UriKind.Relative);

            ApplyTheme();

            DataLossPreventor.Setup(
                this,
                Input,
            hasUnsavedChanges =>
            {
                if (hasUnsavedChanges) { WindowTitleLabel.Content = "*Dzik"; } else { WindowTitleLabel.Content = "Dzik"; }
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
            HandleClipboardContentPasting();
        }

        private void HandleClipboardContentPasting()
        {
            var clipboardText = Clipboard.GetText();

            HandleContentPasting(clipboardText);
        }

        private void HandleContentPasting(string text)
        {
            // Handlers in order, only one can handle the content, then return.
            if (CiphertextHandler.Handle(this, text, msgCryptoTool)) return;
            if (KeyAgreementResponseHandler.Handle(text, keysVault, vault => { AcceptKeysVault(vault); })) return;
            QuotationHandler.Handle(Input, text);
        }

        private void Input_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                var text = (string)e.Data.GetData(DataFormats.UnicodeText);
                HandleContentPasting(text);
                e.Handled = true;
                this.Activate();
                Focus();
            }
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
            else if (e.Key == Key.I && Keyboard.Modifiers == ModifierKeys.Control)
            {
                InsertImageMarker();
                e.Handled = true;
            }
            else if (e.Key == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
            {
                HandleClipboardContentPasting();
                e.Handled = true;
            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MarkSelectionForEncryption();
                e.Handled = true;
            }
            else if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                Input.Redo();
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
            InsertImageMarker();
        }

        private void InsertImageMarker()
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
            MarkSelectionForEncryption();
        }

        private void MarkSelectionForEncryption()
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

        private void LettersButton_Click(object sender, RoutedEventArgs e)
        {
            if (keysVault == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var debateWindow = new DebateWindow(keysVault);
                debateWindow.Show();
            });
        }

        private void ApplyTheme()
        {
            Application.Current.Resources.MergedDictionaries.Remove(DarkTheme);
            Application.Current.Resources.MergedDictionaries.Remove(LightTheme);

            var themeChosen = Settings.Default.Theme;
            ResourceDictionary themeToSet = LightTheme;
            switch (themeChosen)
            {
                case 0: themeToSet = LightTheme; break;
                case 1: themeToSet = DarkTheme; break;
            }

            Application.Current.Resources.MergedDictionaries.Add(themeToSet);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Theme = (Settings.Default.Theme + 1) % 2;
            ApplyTheme();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
