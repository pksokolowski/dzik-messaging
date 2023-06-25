using Dzik.common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Dzik.letter
{
    internal static class RtbTools
    {
        internal static List<Image> GetAllImagesFrom(RichTextBox rtb) => GetAllImagesFrom(rtb.Document);

        internal static List<Image> GetAllImagesFrom(FlowDocument document)
        {
            var images = new List<Image>();

            foreach (Block bk in document.Blocks)
            {
                if (bk is Paragraph)
                {
                    Paragraph par = (Paragraph)bk;
                    foreach (Inline inline in par.Inlines)
                    {
                        // if item is a ui control, like an image
                        if (inline is InlineUIContainer)
                        {
                            InlineUIContainer ui = (InlineUIContainer)inline;
                            if (ui.Child is Image)
                            {
                                Image img = (Image)ui.Child;
                                images.Add(img);
                            }
                        }
                    }
                }
                else if (bk is BlockUIContainer)
                {
                    BlockUIContainer blockui = (BlockUIContainer)bk;
                    if (blockui.Child is Image)
                    {
                        Image img = (Image)blockui.Child;
                        images.Add(img);
                    }
                }
            }

            return images;
        }

        internal static void PlaceBitmapInOutbountRtb(RichTextBox rtb, ImageSource img, int ImagesPerRow, Action<Image> onImageAdded)
        {
            var maxImageWidth = img.Width;
            var imageMargin = 1;

            if (ImagesPerRow > 1)
            {
                var rtbWidth = rtb.ActualWidth;
                var galleryTileWidth = (rtbWidth - 32 - ((ImagesPerRow - 1) * 2 * imageMargin)) / ImagesPerRow;
                maxImageWidth = Math.Min(galleryTileWidth, img.Width);
            }

            Image image = new Image()
            {
                MaxHeight = img.Height,
                MaxWidth = maxImageWidth,
                Margin = new Thickness(imageMargin),
                Source = img
            };

            rtb.BeginChange();

            TextPointer tp = rtb.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            InlineUIContainer imageContainer = new InlineUIContainer(image, tp);

            rtb.CaretPosition = imageContainer.ElementEnd;
            rtb.EndChange();
            rtb.Focus();

            onImageAdded(image);
        }

        internal static long GetImagesTotalImagePixelsCountFrom(FlowDocument document)
        {
            var images = GetAllImagesFrom(document);
            var total = 0L;
            foreach (var image in images)
            {
                total += (long)(image.Height * image.Width);
            }
            return total;
        }

        internal static void PopulateRtbWithBytes(RichTextBox rtb, byte[] bytes, Action<Image> onImageLoaded)
        {
            TextRange range;
            MemoryStream fStream = new MemoryStream();
            fStream.Write(bytes, 0, bytes.Length);

            range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            range.Load(fStream, DataFormats.XamlPackage);
            fStream.Close();

            var images = RtbTools.GetAllImagesFrom(rtb);
            images.ForEach(img =>
            {
                onImageLoaded(img);
            });
        }

        internal static void PlaceStreamedXamlPackageDataIntoTextRange(Stream stream, TextRange textRange)
        {
            textRange.Load(stream, DataFormats.XamlPackage);
            stream.Close();
        }

        internal static byte[] ObtainBytesFrom(RichTextBox rtb)
        {
            TextRange range;
            MemoryStream fStream = new MemoryStream();
            range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            range.Save(fStream, DataFormats.XamlPackage);

            var bytes = fStream.ToArray();

            fStream.Close();
            return bytes;
        }

        internal static void ClearFormatting(RichTextBox rtb)
        {
            ApplyToSelectionAtCarretPos(
                rtb,
                (selection) => selection.ClearAllProperties(),
                () => ReplaceEntireParagraphAtCaretPos(rtb)
            );
        }

        internal static void ReplaceEntireParagraphAtCaretPos(RichTextBox rtb)
        {
            TextPointer carretPos = rtb.CaretPosition;
            if (carretPos.Paragraph == null) return;

            var pStart = carretPos.Paragraph.ContentStart;
            var pEnd = carretPos.Paragraph.ContentEnd;
            var pRange = new TextRange(pStart, pEnd);
            string pText = pRange.Text;

            var carretParagraph = rtb.CaretPosition.Paragraph;

            var run = new Run(pText);
            var paragraph = new Paragraph(run);
            rtb.Document.Blocks.InsertAfter(carretParagraph, paragraph);
            rtb.CaretPosition = paragraph.ContentEnd;

            rtb.Document.Blocks.Remove(carretParagraph);
        }

        internal static void ApplyToSelectionAtCarretPos(RichTextBox rtb, Action<TextSelection> action, Action overrideActionOnNothingSelected = null)
        {
            rtb.BeginChange();
            try
            {
                var selection = rtb.Selection;
                if (!selection.IsEmpty)
                {
                    var selectionRange = new TextRange(selection.Start, selection.End);
                    action(selection);
                }
                else
                {
                    if (overrideActionOnNothingSelected == null)
                    {
                        if (rtb.CaretPosition.Paragraph == null)
                        {
                            var run = new Run(" ");
                            var paragraph = new Paragraph(run);
                            rtb.Document.Blocks.Add(paragraph);

                            rtb.Selection.Select(paragraph.ContentStart, paragraph.ContentEnd);
                            action(rtb.Selection);
                        }
                        else
                        {
                            rtb.Selection.Select(rtb.CaretPosition.Paragraph.ContentStart, rtb.CaretPosition.Paragraph.ContentEnd);
                            if (!rtb.Selection.IsEmpty)
                            {
                                action(rtb.Selection);
                                rtb.Selection.Select(rtb.CaretPosition, rtb.CaretPosition);
                            }
                            else
                            {
                                rtb.CaretPosition.InsertTextInRun(" ");

                                TextPointer tp = rtb.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
                                var posOneLater = tp.GetPositionAtOffset(1, LogicalDirection.Forward);

                                if (posOneLater != null)
                                {
                                    rtb.Selection.Select(tp, posOneLater);
                                }

                                action(rtb.Selection);
                            }
                        }
                    }
                    else
                    {
                        overrideActionOnNothingSelected?.Invoke();
                    }
                }

                rtb.EndChange();
                rtb.Focus();
            }
            catch (Exception)
            {
                rtb.EndChange();
                DialogShower.ShowError("Zmiany formatowania nie powiodły się.");
            }
        }

        internal static void QuoteSelection(RichTextBox sourceRtb, RichTextBox destinationRtb, Brush quotationBackground, Brush accent = null, Brush textColor = null)
        {
            try
            {
                var selection = sourceRtb.Selection.Text.Trim();
                if (selection.Length == 0) return;

                var run = new Run(selection);
                var paragraph = new Paragraph(run);
                paragraph.Background = quotationBackground;
                paragraph.BorderThickness = new Thickness(0, 2, 2, 2);

                if (textColor != null)
                {
                    paragraph.Foreground = textColor;
                }
                else
                {
                    // stay with default color
                }

                if (accent != null)
                {
                    paragraph.BorderBrush = accent;
                }
                else
                {
                    paragraph.BorderBrush = Brushes.DimGray;
                }

                var carretParagraph = destinationRtb.CaretPosition.Paragraph;

                if (carretParagraph == null)
                {
                    destinationRtb.Document.Blocks.InsertBefore(destinationRtb.CaretPosition.InsertParagraphBreak().Paragraph, paragraph);
                }
                else
                if (destinationRtb.CaretPosition.IsAtLineStartPosition)
                {
                    destinationRtb.Document.Blocks.InsertBefore(carretParagraph, paragraph);
                }
                else
                {
                    destinationRtb.Document.Blocks.InsertBefore(destinationRtb.CaretPosition.InsertParagraphBreak().Paragraph, paragraph);
                }

                destinationRtb.CaretPosition = paragraph.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                destinationRtb.Focus();
            }
            catch (Exception)
            {
                DialogShower.ShowError("Wklejenie cytatu nie powiodło się.\n\nPotencjalnie problemem jest niewspierany rodzaj cytowanego contentu lub zawartość prawego pola tekstowego w miejscu, gdzie ostatnio był kursor.");
            }
        }
    }
}
