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

namespace Dzik.letter
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        private BitmapSource currentSwath;
        Action<Brush> onColorSelected;
        Color lastSelectedColorNoBrightness = Colors.White;

        public ColorPickerWindow(string colorTargetHint, Action<Brush> onColorSelected, double defeaultBrightness = 255)
        {
            InitializeComponent();

            TitleLabel.Content = colorTargetHint;
            this.onColorSelected = onColorSelected;

            Loaded += delegate
            {
                InitializeSwath();
                SelectedColorRect.Fill = new SolidColorBrush(lastSelectedColorNoBrightness);
                BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
                BrightnessSlider.Value = defeaultBrightness;
            };
        }

        private void InitializeSwath()
        {
            var source = ColorImage.Source as BitmapSource;

            var digitizerHeight = Digitizer.ActualHeight;
            var digitizerWidth = Digitizer.ActualWidth;
            var bitmap = new TransformedBitmap(source, new ScaleTransform(digitizerWidth / source.PixelWidth, digitizerHeight / source.PixelHeight));

            currentSwath = bitmap;
        }

        private void Digitizer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectNewColor();
            e.Handled = true;
        }

        private void Digitizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;

            SelectNewColor();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SelectedColorRect == null) return;

            var color = lastSelectedColorNoBrightness;
            byte[] channels = { color.B, color.G, color.R, 255 };

            SetNewSelectedColorFromBbrArray(channels);
        }

        private void SelectNewColor()
        {
            try
            {
                var (X, Y) = GetMouseXYOnSwath();
                var sample = ObtainSempleFrom(X, Y);
                SetNewSelectedColorFrom(sample);
                updateSelectionDotPosition(X, Y);
            }
            catch (Exception) { }
        }

        private CroppedBitmap ObtainSempleFrom(double X, double Y)
        {
            var samplingRect = new Int32Rect((int)X, (int)Y, 1, 1);
            var sample = new CroppedBitmap(currentSwath, samplingRect);
            return sample;
        }

        private void SetNewSelectedColorFrom(CroppedBitmap sample)
        {
            var channels = new byte[4];
            sample.CopyPixels(channels, 4, 0);

            SetNewSelectedColorFromBbrArray(channels);

        }

        private void SetNewSelectedColorFromBbrArray(byte[] channels)
        {
            lastSelectedColorNoBrightness = Color.FromArgb(255, channels[2], channels[1], channels[0]);
            var colorAfterBrightnessApplied = ChangeBrightnessInBlueGreenRedArray(channels, BrightnessSlider.Value);
            SelectedColorRect.Fill = new SolidColorBrush(colorAfterBrightnessApplied);
        }

        private Color ChangeBrightnessInBlueGreenRedArray(byte[] channels, double brightness)
        {
            var multiplier = brightness / 255d;

            for (int i = 0; i < 3; i++)
            {
                var cv = channels[i];
                channels[i] = (byte)(cv * multiplier);
            }

            return Color.FromArgb(255, channels[2], channels[1], channels[0]);
        }

        private void updateSelectionDotPosition(double X, double Y)
        {
            var selectedCoordinatesDotSize = ellipsePixel.ActualWidth;
            var halfDotSize = selectedCoordinatesDotSize / 2;

            ellipsePixel.SetValue(Canvas.LeftProperty, (double)(X - halfDotSize));
            ellipsePixel.SetValue(Canvas.TopProperty, (double)(Y - halfDotSize));

            Digitizer.InvalidateVisual();
        }

        private (double, double) GetMouseXYOnSwath()
        {
            var mouseXonCanv = Mouse.GetPosition(Digitizer).X;
            var mouseYonCanv = Mouse.GetPosition(Digitizer).Y;

            return (mouseXonCanv, mouseYonCanv);
        }

        private void AcceptColorButton_Click(object sender, RoutedEventArgs e)
        {
            onColorSelected(SelectedColorRect.Fill);
        }

        private void AcceptColorButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
