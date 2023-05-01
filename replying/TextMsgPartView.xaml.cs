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

namespace Dzik.replying
{
    /// <summary>
    /// Interaction logic for TextMsgPartView.xaml
    /// </summary>
    public partial class TextMsgPartView : UserControl
    {
        private string content;
        public TextMsgPartView(string content)
        {
            InitializeComponent();

            MsgPartLabel.Content = content.Substring(0, Math.Min(content.Length, 40));
            TypeAndLenLabel.Content = $"[TEXT, {content.Length} znaków]";
            this.content = content;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(content);
            msgPartCopiedCheckbox.IsChecked = true;
        }
    }
}
