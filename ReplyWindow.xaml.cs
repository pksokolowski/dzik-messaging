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
            SourceInitialized += MainWindow_SourceInitialized;

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

        // Setup for hiding maximize and minimize buttons on the window.
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        private IntPtr _windowHandle;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            DisableMinimizeButton();
            DisableMaximizeButton();
        }

        protected void DisableMinimizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }

        protected void DisableMaximizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
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

    }
}
