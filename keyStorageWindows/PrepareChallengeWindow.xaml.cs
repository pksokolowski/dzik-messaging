using Dzik.crypto.protocols;
using Dzik.crypto.utils;
using Dzik.data;
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

namespace Dzik.keyStorageWindows
{
    /// <summary>
    /// Interaction logic for PrepareChallengeWindow.xaml
    /// </summary>
    public partial class PrepareChallengeWindow : Window
    {
        public PrepareChallengeWindow()
        {
            InitializeComponent();

            var challengeGenTask = DzikKeyAgreement.GeneratePrivateKeyAndChallengeAndReturnKEK();          
            challengeGenTask.ContinueWith((kek) => {
                Dispatcher.Invoke(new Action(() => {
                    KEKOutputTextBlock.Text = ByteArrayHexStringConverters.MakePresentableStringOf(kek.Result);
                    progressBar.Visibility = Visibility.Collapsed;
                    FinishButton.IsEnabled = true;
                }));
            });            
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            StorageManager.OpenChallengeLocationInExplorer();
        }

        private void KEKOutputTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 3)
            {
                Clipboard.SetText(KEKOutputTextBlock.Text);              
            }
        }
    }
}
