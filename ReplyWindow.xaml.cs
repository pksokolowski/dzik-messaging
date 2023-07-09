using Dzik.Properties;
using Dzik.replying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dzik
{
    /// <summary>
    /// Interaction logic for ReplyWindow.xaml
    /// </summary>
    public partial class ReplyWindow : Window
    {

        internal ReplyWindow(List<MsgPart> msgParts)
        {
            InitializeComponent();

            SourceInitialized += MainWindowRoot_SourceInitialized;
            Closing += MainWindowRoot_Closing;

            msgParts.ForEach(msgPart =>
            {
                switch (msgPart.type)
                {
                    case MsgPartType.TEXT:
                        var txtPartView = new TextMsgPartView(msgPart.content);
                        MsgPartsListBox.Items.Add(txtPartView);
                        break;
                    case MsgPartType.IMG:
                        var imgPartView = new ImgMsgPartView(msgPart.content);
                        MsgPartsListBox.Items.Add(imgPartView);
                        break;
                }

            });
        }

        // Remembering window location
        private void MainWindowRoot_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Settings.Default.ReplyWindowTop;
            this.Left = Settings.Default.ReplyWindowLeft;
        }

        private void MainWindowRoot_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Settings.Default.ReplyWindowTop = RestoreBounds.Top;
                Settings.Default.ReplyWindowLeft = RestoreBounds.Left;
            }
            else
            {
                Settings.Default.ReplyWindowTop = this.Top;
                Settings.Default.ReplyWindowLeft = this.Left;
            }

            Settings.Default.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
