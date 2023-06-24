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
        public LoadingWindow(double ox, double oy, double ow, double oh)
        {
            InitializeComponent();

            this.Left = ox + (0.5 * this.Width);
            this.Top = oy + (0.5 * this.Height);
        }
    }

    class LoadingIndicator
    {
        private LoadingWindow _window;
        private Window _owner;
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

            // clear existing threads:
            CloseLoadingWindows();

            // Create a thread
            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                var window = new LoadingWindow(ox, oy, ow, oh);
                _window = window;

                // Create our context, and install it:
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));


                // When the window closes, shut down the dispatcher
                window.Closed += (s, e) =>
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                window.ShowDialog();

                // Start the Dispatcher Processing
                Dispatcher.Run();
            }));

            // Set the apartment state
            newWindowThread.SetApartmentState(ApartmentState.STA);
            // Make the thread a background thread
            newWindowThread.IsBackground = true;
            // Start the thread
            newWindowThread.Start();
        }

        public bool CloseLoadingWindows()
        {
            if (_window == null) return false;

            _window.Dispatcher.Invoke(new Action(() => _window.Close()));
            _window = null;
            _owner = null;

            return true;
        }
    }
}
