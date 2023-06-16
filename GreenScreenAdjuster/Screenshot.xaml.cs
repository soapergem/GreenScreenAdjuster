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

namespace GreenScreenAdjuster
{
    public partial class Screenshot : Window
    {
        public IntPtr OtherWindow { private get; set; }

        public event Action<Rect> SetCoordinates;
        private Rectangle ResizeFrame;
        private Point? StartingPosition;

        public Screenshot()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            User32.SetForegroundWindow(OtherWindow);

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            Left = 0;
            Top = 0;
            Width = screenWidth;
            Height = screenHeight;

            await Task.Delay(500);
            MainCanvas.Visibility = Visibility.Visible;
            Activate();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(MainCanvas);
            StartingPosition = position;

            ResizeFrame = new Rectangle
            {
                Height = 0,
                Width = 0,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red,
                StrokeThickness = 3,
            };

            MainCanvas.Children.Add(ResizeFrame);
            Canvas.SetLeft(ResizeFrame, position.X);
            Canvas.SetTop(ResizeFrame, position.Y);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ResizeFrame != null && StartingPosition != null)
            {
                var position = e.GetPosition(MainCanvas);
                Canvas.SetLeft(ResizeFrame, Math.Min(position.X, StartingPosition.Value.X));
                Canvas.SetTop(ResizeFrame, Math.Min(position.Y, StartingPosition.Value.Y));
                ResizeFrame.Height = Math.Abs(position.Y - StartingPosition.Value.Y);
                ResizeFrame.Width = Math.Abs(position.X - StartingPosition.Value.X);
            }
        }

        public static double GetSystemScaleFactor()
        {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            Matrix matrix = source.CompositionTarget.TransformToDevice;

            double dpiX = 96.0 * matrix.M11;
            double dpiY = 96.0 * matrix.M22;

            return dpiX / 96.0;
        }

        private Rect GetCoordinates()
        {

            var x = Canvas.GetLeft(ResizeFrame);
            var y = Canvas.GetTop(ResizeFrame);
            var sf = GetSystemScaleFactor();
            return new Rect
            {
                Left = (int) Math.Floor(x * sf),
                Top = (int) Math.Floor(y * sf),
                Right = (int) Math.Floor((x + ResizeFrame.Width) * sf),
                Bottom = (int) Math.Floor((y + ResizeFrame.Height) * sf)
            };
        }

        private async void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ResizeFrame != null)
            {
                SetCoordinates?.Invoke(GetCoordinates());
                ResizeFrame = null;
                StartingPosition = null;
                MainCanvas.Children.Remove(ResizeFrame);
                await Task.Delay(250);
                Close();
            }
        }

        private void Canvas_TouchDown(object sender, TouchEventArgs e)
        {
            if (ResizeFrame != null)
            {
                var position = e.GetTouchPoint(MainCanvas);
            }        
        }

        private void Canvas_TouchMove(object sender, TouchEventArgs e)
        {

        }

        private void Canvas_TouchUp(object sender, TouchEventArgs e)
        {

        }
    }
}
