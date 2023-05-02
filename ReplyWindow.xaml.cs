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

        private void Window_StateChanged(object sender, EventArgs e)
        {          
            try
            {
                var window = (sender as Window);
                if (window.WindowState == WindowState.Minimized || window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
            }
            catch
            {

            }
        }
    }
}
