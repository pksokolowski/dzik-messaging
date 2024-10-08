﻿using Dzik.common;
using Dzik.crypto.protocols;
using Dzik.crypto.utils;
using Dzik.data;
using Dzik.letter.estimators;
using Dzik.letters.utils;
using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dzik.letter
{
    /// <summary>
    /// Interaction logic for DebateWindow.xaml
    /// </summary>
    public partial class DebateWindow : Window
    {
        private readonly KeysVault keysVault;

        private int ImagesPerRow;

        private bool needToSaveInboundMessage = true;
        private byte[] inboundMessageBytes = null;

        private bool hasPotentiallyUnsavedChanges = false;
        private List<Window> toolWindowsOpened = new List<Window>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vault">a vault containing secret symmetric keys and allowing to perform cryptographic operations with them.</param>
        /// <param name="messageReceived">a message bytes to be displayed, should not contain the Xaml message mark (must be removed before)</param>
        public DebateWindow(KeysVault vault, byte[] messageReceived = null)
        {
            InitializeComponent();
            ImgUtils.SetHighQualityScalingFor(inboundRtb);
            ImgUtils.SetHighQualityScalingFor(outboundRtb);

            this.keysVault = vault;

            setImageSizingMode(2);

            if (messageReceived != null)
            {
                PopulateRtbWithBytes(inboundRtb, messageReceived);
                inboundMessageBytes = messageReceived;            
            }
            else if (XamlMessageDraftStorage.HasStoredDraft())
            {
                // since inbound message is readOnly, if it's present, no need to save it again.
                needToSaveInboundMessage = false;

                var loadingIndicator = new LoadingIndicator(
                 owner: this,
                 location: LoadingIndicatorLocation.CenterScreen
                 );

                var (inbound, outbound) = XamlMessageDraftStorage.LoadDraftState(keysVault);             

                if (inbound == null)
                {
                    ShowInstructionManual();
                }
                else
                {
                    PopulateRtbWithBytes(inboundRtb, inbound);
                }
                if (outbound != null)
                {
                    PopulateRtbWithBytes(outboundRtb, outbound);
                    hasPotentiallyUnsavedChanges = false;
                }
                loadingIndicator.CloseIndicator();
            }
            else
            {
                ShowInstructionManual();
            }

            DataObject.AddPastingHandler(outboundRtb, OnPaste);

            ((App)Application.Current).SessionEnding += DebateWindow_SessionEnding;

            IndicateChosenQuotationColor();
        }

        private void DebateWindow_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (!hasPotentiallyUnsavedChanges) return;
            //if (App.ForbidWindowClosingPrevention) return;

            e.Cancel = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!hasPotentiallyUnsavedChanges) return;
            //if (App.ForbidWindowClosingPrevention) return;

            var usersChoice = DialogShower.ShowSaveExitCancelDialog("Edytor Listu", "Czy chcesz zapisać zmiany?");
            switch (usersChoice)
            {
                case DialogShower.UsersChoice.yes:
                    var successfullySavedDraft = SaveDraft();
                    if (!successfullySavedDraft)
                    {
                        e.Cancel = true;
                    }
                    break;
                case DialogShower.UsersChoice.no:
                    break;
                case DialogShower.UsersChoice.cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //// prevent any hard to diagnoze crashes, other windows will soon be killed probably anyway.
            //if (App.ForbidWindowClosingPrevention) return;

            ((App)Application.Current).SessionEnding -= DebateWindow_SessionEnding;

            toolWindowsOpened.ForEach(window =>
            {
                RemoveCallbacksFromToolWindow(window);
                window.Close();
            });

            Settings.Default.Save();
        }

        private void outboundRtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasPotentiallyUnsavedChanges = true;
        }

        private void ShowInstructionManual() => RtbManualProvider.DisplayXamlMessageManualIn(inboundRtb);

        private void CommandBinding_CanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command != ApplicationCommands.Paste) return;

            e.CanExecute = true;
            e.Handled = true;
        }

        private void outboundRtb_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command != ApplicationCommands.Paste) return;

            try
            {
                var files = Clipboard.GetFileDropList();
                if (files.Count > 0)
                {
                    var filesArray = files.Cast<string>().ToArray();
                    PlaceImagesFromFilesInOutboundRtb(filesArray);

                    e.Handled = true;
                }
            }
            catch (Exception) { }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                if (e.DataObject.GetDataPresent(DataFormats.Bitmap) && e.DataObject.GetDataPresent(DataFormats.XamlPackage))
                { // this is the case when bitmaps were growing in size insanely, now its blocked.
                    e.CancelCommand();
                    Focus();
                }
                else if (e.DataObject.GetDataPresent(DataFormats.XamlPackage))
                {
                    //var data = e.DataObject.GetData(DataFormats.XamlPackage);
                    //if (data != null)
                    //{
                    //    RtbTools.PlaceStreamedXamlPackageDataIntoTextRange(data as Stream, new TextRange(outboundRtb.Selection.Start, outboundRtb.Selection.End));
                    //    e.Handled = true;
                    //}
                    e.Handled = true;
                }
                else if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
                {
                    PlaceBitmapInOutboundRtb(e.DataObject, () =>
                    {
                        e.CancelCommand();
                        return;
                    });

                    e.CancelCommand();
                    Focus();
                }
                else if (e.DataObject.GetDataPresent(DataFormats.Xaml))
                {
                    e.Handled = false;
                }
                else if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
                {
                    var text = (string)e.DataObject.GetData(DataFormats.UnicodeText);
                    outboundRtb.Selection.Text = text.Replace("\n", "");
                    outboundRtb.CaretPosition = outboundRtb.Selection.End;
                    e.CancelCommand();
                    Focus();
                }
            }
            catch (Exception) { }
        }

        private void PlaceBitmapInOutboundRtb(IDataObject dataObject, Action onNullImage)
        {
            var dataObj = dataObject.GetData(DataFormats.Bitmap);
            if (dataObj is BitmapImage)
            {
                // unhandled type, ignore.
                return;
            }
            var img = (InteropBitmap)dataObj;
            if (img == null)
            {
                onNullImage();
                return;
            }
            PlaceBitmapInOutbountRtb(img);
        }

        private void PlaceBitmapInOutbountRtb(ImageSource img) => RtbTools.PlaceBitmapInOutbountRtb(outboundRtb, img, ImagesPerRow, SetOnClickListener);

        private void PlaceImagesFromFilesInOutboundRtb(string[] files)
        {
            var images = new List<ImageSource>();
            foreach (string file in files)
            {
                try
                {
                    var imageSource = ImgUtils.LoadImageWithCorrectRotation(file);
                    images.Add(imageSource);
                }
                catch (Exception) { }
            }

            var sortedImages = images.OrderBy(img => img.Height);
            foreach (ImageSource img in sortedImages)
            {
                try
                {
                    PlaceBitmapInOutbountRtb(img);
                }
                catch (Exception) { }
            }
        }

        private void outboundRtb_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                PlaceImagesFromFilesInOutboundRtb(files);

                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                PlaceBitmapInOutboundRtb(e.Data, () => { });
                e.Handled = true;
            }

            Activate();
            Focus();
        }

        private void ImageMouseLEftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            var img = (Image)sender;
            ImageViewerWindow imgView = new ImageViewerWindow(img);
            imgView.Show();
            e.Handled = true;
        }

        private void SetOnClickListener(Image img)
        {
            img.MouseLeftButtonDown += ImageMouseLEftButtonDownHandler;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void outboundRtb_PreviewDragOver_1(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void PopulateRtbWithBytes(RichTextBox rtb, byte[] bytes) => RtbTools.PopulateRtbWithBytes(rtb, bytes, SetOnClickListener);

        private byte[] ObtainBytesFrom(RichTextBox rtb) => RtbTools.ObtainBytesFrom(rtb);

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void QuoteSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            QuoteSelection();
        }

        private void quotationGrayEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Settings.Default.LetterQuotationTheme = 0;
            IndicateChosenQuotationColor();
        }

        private void quotationPinkEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Settings.Default.LetterQuotationTheme = 1;
            IndicateChosenQuotationColor();
        }

        private void quotationBlueEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Settings.Default.LetterQuotationTheme = 2;
            IndicateChosenQuotationColor();
        }

        private void IndicateChosenQuotationColor()
        {
            var quotationThemeCode = Settings.Default.LetterQuotationTheme;
            var ellipses = new Ellipse[] { quotationGrayEllipse, quotationPinkEllipse, quotationBlueEllipse };

            for (int i = 0; i < ellipses.Length; i++)
            {
                if (quotationThemeCode == i)
                {
                    ellipses[i].Opacity = 1;
                }
                else
                {
                    ellipses[i].Opacity = 0.5;
                }
            }
        }

        private void QuoteSelection()
        {
            var converter = new BrushConverter();
            Brush bg;
            Brush stroke;
            Brush txt;

            switch (Settings.Default.LetterQuotationTheme)
            {
                case 1:
                    bg = outboundRtb.Background;
                    stroke = (Brush)converter.ConvertFrom("#FFbf48a7");
                    txt = (Brush)converter.ConvertFrom("#FFF095FF");
                    break;
                case 2:
                    bg = outboundRtb.Background;
                    stroke = (Brush)converter.ConvertFrom("#FF4383af");
                    txt = Brushes.LightSkyBlue;
                    break;
                default:
                    bg = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                    stroke = null;
                    txt = null;
                    break;
            }
            RtbTools.QuoteSelection(inboundRtb, outboundRtb, bg, stroke, txt);
        }

        private void EncryptToFileButton_Click(object sender, RoutedEventArgs e)
        {
            var loadingIndicator = new LoadingIndicator(this);

            var outputPath = LettersStorageManager.GetOutBoundPath();

            try
            {
                var messageBytes = ObtainBytesFrom(outboundRtb);
                var encryptionToFileResult = FileEncryptionTool.EncryptXamlMessageToFile(outputPath, keysVault, messageBytes);

                if (encryptionToFileResult != FileCryptoOperationResult.encrypted)
                {
                    DialogShower.ShowError("Szyfrowanie wiadomości nie powiodło się");
                    return;
                }

                hasPotentiallyUnsavedChanges = false;
                WindowsExplorerOpener.ShowFileInExplorer(outputPath);
            }
            catch (Exception)
            {
                DialogShower.ShowError($"Przygotowanie zaszyfrowanego pliku i/lub otwarcie folderu go zawierającego nie powiodło się.\n\nSprawdź '{outputPath}' aby potwierdzić, czy plik wiadomości został wygenerowany. Zaleca się próbne odszyfrowanie pliku, dla pewności, przed wysłaniem.");
            }
            finally
            {
                loadingIndicator.CloseIndicator();
            }
        }

        private byte[] ObtainInboundBytesEfficiently()
        {
            if (inboundMessageBytes == null)
            {
                return ObtainBytesFrom(inboundRtb);
            }
            return inboundMessageBytes;
        }

        private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDraft();
        }

        private bool SaveDraft()
        {
            var loadingIndicator = new LoadingIndicator(this);
            try
            {
                var inbound = needToSaveInboundMessage ? ObtainInboundBytesEfficiently() : null;
                var outbound = ObtainBytesFrom(outboundRtb);

                var storingSuccessful = XamlMessageDraftStorage.StoreDraftState(inbound, outbound, keysVault);
                if (!storingSuccessful)
                {
                    DialogShower.ShowError("Nie udało się zapisać kopii roboczej wiadomości.\n\nSpróbuj zaszyfrować wiadomość ręcznie i zapisać ją jako backup samodzielnie.");
                }
                needToSaveInboundMessage = false;
                hasPotentiallyUnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                DialogShower.ShowError("Nie udało się zapisać kopii roboczej wiadomości.");
                return false;
            }
            finally
            {
                loadingIndicator.CloseIndicator();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setImageSizingMode(1);
        }

        private void ImagesPerRow_2Button_Click(object sender, RoutedEventArgs e)
        {
            setImageSizingMode(2);
        }

        private void ImagesPerRow_3Button_Click(object sender, RoutedEventArgs e)
        {
            setImageSizingMode(3);
        }

        private void ImagesPerRow_4Button_Click(object sender, RoutedEventArgs e)
        {
            setImageSizingMode(4);
        }

        private void setImageSizingMode(int maxImagesPerRow)
        {
            ImagesPerRow = maxImagesPerRow;

            Button[] buttons = { ImagesPerRow_1Button, ImagesPerRow_2Button, ImagesPerRow_3Button, ImagesPerRow_4Button };
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                var buttonsImagesPerRowValue = i + 1;
                if (buttonsImagesPerRowValue == maxImagesPerRow)
                {
                    button.Background = Brushes.Gray;
                }
                else
                {
                    button.Background = null;
                }
            }
        }

        private void ClearFormattingButton_Click(object sender, RoutedEventArgs e)
        {
            RtbTools.ClearFormatting(outboundRtb);
        }

        private void ForegroundColorButton_Click(object sender, RoutedEventArgs e)
        {
            var cp = new ColorPickerWindow("Tekst", (color) =>
            {
                RtbTools.ApplyToSelectionAtCarretPos(outboundRtb, (selection) => selection.ApplyPropertyValue(TextElement.ForegroundProperty, color));
            });
            cp.Show();
            RegisterToolWindow(cp);
        }

        private void BackgroundColorButton_Click(object sender, RoutedEventArgs e)
        {
            var cp = new ColorPickerWindow("Tło", (color) =>
            {
                RtbTools.ApplyToSelectionAtCarretPos(outboundRtb, (selection) => selection.ApplyPropertyValue(TextElement.BackgroundProperty, color));
            }, defeaultBrightness: 150);
            cp.Show();
            RegisterToolWindow(cp);
        }

        private void RegisterToolWindow(Window tw)
        {
            toolWindowsOpened.Add(tw);
            tw.Closed += ToolWindow_Closed;
        }

        private void RemoveCallbacksFromToolWindow(Window tw)
        {
            tw.Closed -= ToolWindow_Closed;
        }

        private void ToolWindow_Closed(object sender, EventArgs e)
        {
            if (sender == null) return;
            try
            {
                toolWindowsOpened.Remove(sender as Window);
            }
            catch (Exception) { }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveDraft();
                e.Handled = true;
            }
            else if (e.Key == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
            {
                QuoteSelection();
                e.Handled = true;
            }
        }

        private void outboundRtb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyUp(Key.RightAlt))
            {
                outboundRtb.Redo();
                e.Handled = true;
            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                RtbTools.ClearFormatting(outboundRtb);
                e.Handled = true;
            }
        }
    }
}
