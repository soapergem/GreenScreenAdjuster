using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;

namespace GreenScreenAdjuster
{
    public partial class MainWindow : Window
    {
        private Rect? Bounds { get; set; }
        private Rect? WindowBounds { get; set; }
        private Dictionary<IntPtr, string> ExternalWindows;
        private Color StoredColor { get; set; }
        private Window screenshot;
        protected OBSWebsocket obs;
        protected BrushConverter brushConverter;
        protected DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            brushConverter = new BrushConverter();
            timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(5), IsEnabled = false};
            timer.Tick += Timer_Tick;
            obs = new OBSWebsocket();
            obs.Connected += OnConnected;
            obs.Disconnected += OnDisconnected;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            var currentWindow = User32.GetForegroundWindow();

            CheckForChangedBounds();

            var windowHandle = GetSelectedWindow();
            User32.SetForegroundWindow(windowHandle);
            await Task.Delay(250);

            var hexColor = GetAverageColor(Bounds.Value);
            StoredColor = Color.FromHexCode(hexColor);

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                HexColor.Content = hexColor;
                ColorRect.Fill = (System.Windows.Media.Brush)brushConverter.ConvertFromString("#" + hexColor);
            }));

            await Task.Delay(250);

            User32.SetForegroundWindow(currentWindow);
            UpdateColorSettings(StoredColor);
        }

        private void CheckForChangedBounds()
        {
            var currentWindowBounds = Utilities.GetWindowRect(GetSelectedWindow());
            if (currentWindowBounds != WindowBounds.Value)
            {
                if (currentWindowBounds.SizeEquals(WindowBounds.Value))
                {
                    var xDelta = currentWindowBounds.Left - WindowBounds.Value.Left;
                    var yDelta = currentWindowBounds.Top - WindowBounds.Value.Top;
                    var newBounds = new Rect
                    {
                        Left = (int)Bounds?.Left + xDelta,
                        Right = (int)Bounds?.Right + xDelta,
                        Top = (int)Bounds?.Top + yDelta,
                        Bottom = (int)Bounds?.Bottom + yDelta,
                    };
                    Bounds = newBounds;
                    WindowBounds = currentWindowBounds;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExternalWindows = Utilities.GetOpenWindows();
            WindowList.ItemsSource = ExternalWindows.Values;
            ObsPassword.Password = Properties.Settings.Default.ObsPassword;
            if (ExternalWindows.Values.Contains("Logi Capture"))
            {
                WindowList.SelectedItem = "Logi Capture";
            }
        }

        private void ObsPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ObsPassword = ObsPassword.Password;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ExternalWindows = Utilities.GetOpenWindows();
            WindowList.ItemsSource = ExternalWindows.Values;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!obs.IsConnected)
            {
                Status.Content = "Trying to connect...";
                try
                {
                    obs.ConnectAsync("ws://" + ObsIp.Text + ":4455", ObsPassword.Password);
                }
                catch (Exception ex)
                {
                    Status.Content = ex.Message;
                }
            }
            else
            {
                obs.Disconnect();
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Connect.Content = "Disconnect";
                Status.Content = "Connected successfully";
            }));
        }

        private void OnDisconnected(object sender, ObsDisconnectionInfo e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Connect.Content = "Connect";
                Status.Content = "Disconnected";
            }));
        }

        private IntPtr GetSelectedWindow()
        {
            var windowTitle = WindowList.SelectedItem.ToString();
            return ExternalWindows.FirstOrDefault(x => x.Value == windowTitle).Key;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Bounds.HasValue)
            {
                // stop button
                Bounds = null;
                ActionButton.Content = "Take Screenshot";
                ColorRect.Visibility = Visibility.Hidden;
                HexColor.Content = "";
                StoredColor = null;
                timer.Stop();
            }
            else
            {
                // take screenshot button
                screenshot = new Screenshot { OtherWindow = GetSelectedWindow() };
                ((Screenshot)screenshot).SetCoordinates += Screenshot_SetCoordinates;
                screenshot.Closed += Screenshot_Closed;
                screenshot.Show();
            }
        }

        private void Screenshot_Closed(object sender, EventArgs e)
        {
            ((Screenshot)screenshot).SetCoordinates -= Screenshot_SetCoordinates;
            screenshot.Closed -= Screenshot_Closed;
            screenshot = null;
        }

        private async void Screenshot_SetCoordinates(Rect dimensions)
        {
            WindowBounds = Utilities.GetWindowRect(GetSelectedWindow());
            Bounds = dimensions;

            await Task.Delay(500);
            await Task.Run(() =>
            {
                var hexColor = GetAverageColor(dimensions);
                StoredColor = Color.FromHexCode(hexColor);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Activate(); // TODO: comment me out
                    ActionButton.Content = "Stop";
                    HexColor.Content = hexColor;
                    ColorRect.Fill = (System.Windows.Media.Brush)brushConverter.ConvertFromString("#" + hexColor);
                    ColorRect.Visibility = Visibility.Visible;
                    timer.Start();
                }));
            });
        }

        private void UpdateColorSettings(Color color)
        {
            if (obs.IsConnected)
            {
                var settings = obs.GetSourceFilter(Source.Text, Filter.Text);
                var actualSettings = settings.Settings;
                actualSettings["key_color"] = color.ToObsUInt();
                obs.SetSourceFilterSettings(Source.Text, Filter.Text, actualSettings);
            }
        }

        private string GetAverageColor(Rect bounds)
        {
            var screenBounds = new Rectangle
            {
                X = bounds.Left,
                Y = bounds.Top,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
            };

            // Create a bitmap with the screen size
            using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                // Create a Graphics object from the bitmap
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Copy the screen to the bitmap
                    graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);
                }

                var hexColor = ImgUtils.GetAverageColor(bitmap);
                return hexColor;
            }
        }
    }
}