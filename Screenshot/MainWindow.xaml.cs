using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Drawing.Point;
using Screen = System.Windows.Forms.Screen;
using Size = System.Drawing.Size;

namespace Screenshot
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private PerformanceCounter _cpuCounter;

        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            InitPerformanceCounter();
        }

        private void InitTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        private void InitPerformanceCounter()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DisplayCpuUsage();
            UpdateScreenShotImageNonBlocking();
        }

        private void DisplayCpuUsage()
        {
            float cpuUsage = _cpuCounter.NextValue();
            SystemStateLabel.Content = $"{cpuUsage:F2}%";
        }

        private void ButtonTakeScreenShot_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateScreenShotImageNonBlocking();
        }

        private void UpdateScreenShotImageNonBlocking()
        {
            // No implementation provided in original code, so keeping it as is

        }

        private void UpdateScreenShotImage()
        {
            var selectedScreen = (Screen)MonitorComboBox.SelectedItem;
            if (selectedScreen == null) return;

            var screenshot = TakeScreenshot(selectedScreen);
            ScreenShotImage.Source = ConvertBitmapToBitmapImage(screenshot);
        }

        private Bitmap TakeScreenshot(Screen selectedScreen)
        {
            using var bitmap = new Bitmap(selectedScreen.Bounds.Width, selectedScreen.Bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(selectedScreen.Bounds.X, selectedScreen.Bounds.Y, 0, 0, bitmap.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            // Save the bitmap to a memory stream
            using var memoryStream = new System.IO.MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            // Create a BitmapImage from the memory stream
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // To make the BitmapImage usable in other threads

            return bitmapImage;
        }

        private void MonitorComboBox_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Get all screens and add them to the ComboBox
            var screens = Screen.AllScreens;
            foreach (var screen in screens)
            {
                MonitorComboBox.Items.Add(screen);
            }

            // Select the first screen by default
            if (MonitorComboBox.Items.Count > 0)
            {
                MonitorComboBox.SelectedIndex = 0;
            }
        }
    }
}