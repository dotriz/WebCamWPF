using AForge.Video.DirectShow;
using CamHelper;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AForge.Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCaptureDevice _videoSource;
        public MainWindow()
        {
            InitializeComponent();
            WebCamSetup();
            Closing += Window_Closing;
        }

        #region WEBCam Implementation

        #region Public properties

        private readonly DependencyProperty CurrentFrameProperty = DependencyProperty.Register("CurrentFrame", typeof(BitmapImage), typeof(MainWindow), new FrameworkPropertyMetadata(null));
        public BitmapImage CurrentFrame
        {
            get { return (BitmapImage)GetValue(CurrentFrameProperty); }
            set
            {
                SetValue(CurrentFrameProperty, value);
            }
        }

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        private FilterInfo _currentDevice;
        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }

        #endregion

        public void WebCamSetup()
        {
            GetVideoDevices();
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        bool WebCamInitialised = false;
        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = ToBitmapImage(bitmap);
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.Invoke(() =>
                {
                    CurrentFrame = bi;
                });

                if (!WebCamInitialised) WebCamInitialised = true;
            }
            catch (Exception exc)
            {
                StopCamera();
            }
        }

        private void GetVideoDevices()
        {
            VideoDevices = new WebCamController().VideoDevices;
            
            if (VideoDevices.Any()) 
            {
                CurrentDevice = VideoDevices[0];
                comboBox.ItemsSource = VideoDevices;
                comboBox.SelectedItem = CurrentDevice;
            }
            else
            {
                //MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                try
                {
                    _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                    _videoSource.NewFrame += video_NewFrame;
                    VideoCapabilities SelectedVC = _videoSource.VideoCapabilities[0];
                    string availableOption = CurrentDevice.Name + " ... ";
                    foreach (var cap in _videoSource.VideoCapabilities)
                    {
                        availableOption += cap.FrameSize.Width + "x" + cap.FrameSize.Height + " at " + cap.AverageFrameRate + " ... ";
                        if (cap.FrameSize.Height == 720)
                        {
                            SelectedVC = cap;
                            break;
                        }
                    }
                    JSLog.Log("Available Fallback Camera Resolution: " + availableOption, JSLog.LogType.INFO);

                    _videoSource.VideoResolution = SelectedVC;
                    JSLog.Log("Selected Resolution: " + SelectedVC.FrameSize.Width + "x" + SelectedVC.FrameSize.Height + " at " + SelectedVC.AverageFrameRate, JSLog.LogType.INFO);
                    _videoSource.Start();
                }
                catch (Exception e)
                {
                    //Unable to start camera
                    JSLog.Log("Fallback Camera Exception: " + e.ToString(), JSLog.LogType.INFO);
                }
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= video_NewFrame;
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            CurrentDevice = (FilterInfo)cmb.SelectedItem;
            StopCamera();
            StartCamera();
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopCamera();  
        }

    }

}
