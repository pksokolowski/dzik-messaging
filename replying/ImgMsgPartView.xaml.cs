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
    /// Interaction logic for ImgMsgPartView.xaml
    /// </summary>
    public partial class ImgMsgPartView : UserControl
    {
        private string content;
        public ImgMsgPartView(string description)
        {
            InitializeComponent();
            DescriptionLabel.Content = description;
            this.content = description.Substring(Constants.MARKER_IMAGE_TAG.Length).TrimStart();
        }

        private void CopyButton_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(content);
            wasCopiedCheckbox.IsChecked = true;
        }
    }
}
