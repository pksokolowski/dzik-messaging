using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Dzik.letter
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow(Window owner)
        {
            InitializeComponent();

            double ox = owner.Left;
            double oy = owner.Top;
            double ow = owner.Width;
            double oh = owner.Height;

            this.Left = ox + (0.5 * this.Width);
            this.Top = oy + (0.5 * this.Height);
        }

        public LoadingWindow(double ox, double oy, double ow, double oh)
        {
            InitializeComponent();

            this.Left = ox + (0.5 * this.Width);
            this.Top = oy + (0.5 * this.Height);
        }

        public static void ShowIndicator(Window owner)
        {
            double ox = owner.Left;
            double oy = owner.Top;
            double ow = owner.Width;
            double oh = owner.Height;

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                var window = new LoadingWindow(ox, oy, ow, oh);
                _window = window;
                window.ShowDialog();
                owner.Dispatcher.Invoke(() => owner.Activate());
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();


        }

        private static LoadingWindow _window;

        public static void CloseWindowSafely()
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

    class LoadingIndicator
    {
        private LoadingWindow _window;
        private Window _owner;
        private bool _markedForClosing = false;
        public LoadingIndicator(Window owner)
        {
            _owner = owner;
            ShowLoadingDialog();
        }

        private void ShowLoadingDialog()
        {
            double ox = _owner.Left;
            double oy = _owner.Top;
            double ow = _owner.Width;
            double oh = _owner.Height;

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                var window = new LoadingWindow(ox, oy, ow, oh);
                _window = window;
                window.ShowDialog();
                _owner.Dispatcher.Invoke(() => _owner.Activate());
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

        }

        public void CloseLoadingWindows()
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
