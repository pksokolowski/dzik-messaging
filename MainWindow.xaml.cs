using Dzik.editing;
using Dzik.replying;
using System;
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
        public MainWindow()
        {
            InitializeComponent();

            EditorStartBehavior.InitializeEditor(Input);
            DataLossPreventor.Setup(this, Input);
        }

        private void QuoteButton_Click(object sender, RoutedEventArgs e)
        {
            var initialSelectionStart = Input.SelectionStart;
            var clipboardText = Clipboard.GetText();
            var quoted = LinesPrepender.Prepended(clipboardText, "> ", true);
            if (!LinesPrepender.IsCarretAtTheBeginningOfLine(Input)) quoted = '\n' + quoted;

            // add line breaks after, for convenience
            quoted += "\n\n";
            ContentPaster.PasteInto(Input, quoted);
            Input.SelectionStart = initialSelectionStart + quoted.Length;
            Input.Focus();
        }

        private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
        {
            DraftStorage.Store(Input);
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
            DraftStorage.Store(Input);

            var msgParts = ReplyAssembler.Assemble(Input.Text);

            var replyWindow = new ReplyWindow(msgParts);
            replyWindow.Show();
        }
    }
}
