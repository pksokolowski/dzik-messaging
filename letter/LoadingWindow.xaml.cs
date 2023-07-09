using Dzik.letter.estimators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Animation;
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
            Opacity = 0;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AnimateEntrance();
        }

        public LoadingWindow(double ox, double oy, double ow, double oh)
        {
            Opacity = 0;
            InitializeComponent();

            this.Left = ox + (0.5 * ow) - (0.5 * this.Width);
            this.Top = oy + (0.5 * oh) - (0.5 * this.Height);

            AnimateEntrance();
        }

        private void AnimateEntrance()
        {
            Opacity = 0;

            BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500)
            });
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
        private Estimator _estimator;
        private bool _shouldActivateOwnerWindowAfterwards;
        private bool _aborted = false;

        public LoadingIndicator(Window owner, LoadingIndicatorLocation location = LoadingIndicatorLocation.CenterOwner, bool shouldActivateOwnerAfterwards = true, Estimator estimator = null)
        {
            _estimator = estimator;
            _owner = owner;
            _shouldActivateOwnerWindowAfterwards = shouldActivateOwnerAfterwards;

            estimator?.Start();

            if (estimator != null)
            {
                var estimateMillis = estimator.GetEstimatedMillis();
                if (estimateMillis < 1000)
                {
                    _aborted = true;
                    return;
                }
            }

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

                if (_shouldActivateOwnerWindowAfterwards)
                {
                    _owner.Dispatcher.Invoke(() => _owner.Activate());
                }
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = false;
            newWindowThread.Start();

        }

        public void CloseIndicator()
        {
            _estimator?.End();

            if (_aborted) return;

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
