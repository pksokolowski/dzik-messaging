using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using static Dzik.letter.LoadingIndicator;

namespace Dzik.letter
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public LoadingWindow(double ox, double oy, double ow, double oh)
        {
            InitializeComponent();

            this.Left = ox + (0.5 * ow) - (0.5 * this.Width);
            this.Top = oy + (0.5 * oh) - (0.5 * this.Height);
        }
    }

    public enum LoadingIndicatorLocation
    {
        CenterScreen,
        CenterOwner,
    }

    class LoadingIndicator
    {
        private LoadingWindow _window;
        private Window _owner;
        private bool _markedForClosing = false;
        public LoadingIndicator(Window owner, LoadingIndicatorLocation location = LoadingIndicatorLocation.CenterOwner)
        {
            _owner = owner;
            ShowLoadingDialog(location);
        }

        private void ShowLoadingDialog(LoadingIndicatorLocation location = LoadingIndicatorLocation.CenterOwner)
        {
            double ox = _owner.Left;
            double oy = _owner.Top;
            double ow = _owner.Width;
            double oh = _owner.Height;

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                if (location == LoadingIndicatorLocation.CenterOwner)
                {
                    _window = new LoadingWindow(ox, oy, ow, oh);
                }
                else
                {
                    _window = new LoadingWindow();
                }
         
                _window.ShowDialog();
                _owner.Dispatcher.Invoke(() => _owner.Activate());
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

        }

        public void CloseIndicator()
        {
            while (_window == null)
            {
                Thread.Sleep(50);
            }

            if (_window.Dispatcher.CheckAccess())
                _window.Close();
            else
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(_window.Close));
        }
    }
}
