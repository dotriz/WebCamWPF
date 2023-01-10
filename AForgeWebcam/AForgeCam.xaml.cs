using AForge.Video.DirectShow;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
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

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        private FilterInfo _currentDevice;
        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }

        public string SelectedWebcam { get; set; }
        public int WebCamPixelWidth { get; private set; }
        public int WebCamPixelHeight { get; private set; }


        #endregion


        #region Private fields

        double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        double ScreenHeight = SystemParameters.PrimaryScreenHeight;
        double MaxScreenWidth = SystemParameters.PrimaryScreenWidth;
        double MaxScreenHeight = SystemParameters.PrimaryScreenHeight;

        #endregion

        public void WebCamSetup()
        {
            SelectedWebcam = "";
            this.DataContext = this;
            GetVideoDevices();

            //StartCamera();
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
        //private DispatcherTimer timerEnter, timerLeave;
        //private bool IsMouseOverWebCamPlayerButtons = false;
#if DEBUG
        private System.Diagnostics.Stopwatch frameTimer = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch frameTotalTimer = new System.Diagnostics.Stopwatch();
        private long frameCount = 0, frameTotalMilliSeconds = 0;
        private readonly bool logFramePerformance = true;
#endif
        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
#if DEBUG
                if (logFramePerformance) { frameCount++; frameTimer.Start(); frameTotalTimer.Start(); }
#endif
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = ToBitmapImage(bitmap);
                    WebCamPixelWidth = bi.PixelWidth; //320
                    WebCamPixelHeight = bi.PixelHeight; //240
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    WebCamPlayerCircle.ImageSource = bi;
#if DEBUG
                    if (logFramePerformance)
                    {
                        frameTotalMilliSeconds += frameTimer.ElapsedMilliseconds;
                        var sec = (frameTotalTimer.ElapsedMilliseconds / 1000);
                        if (sec < 1) sec = 1;
                        Console.WriteLine(
                            "FPS: " + (frameCount/sec) + 
                            " -- One Frame Processing:" + frameTimer.ElapsedMilliseconds +
                            " -- AVG Frame Processing: " + ((float)frameTotalMilliSeconds / frameCount) +
                            " -- Frame Count:" + frameCount);
                        frameTimer.Stop(); frameTimer.Reset();
                    }
#endif
                    if (!WebCamInitialised)
                    {
                        WebCamInitialised = true;
                    }
                }));
            }
            catch (Exception exc)
            {
                //TODO - decide what to do here?
                //MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }
        private void GetVideoDevices()
        {
            VideoDevices = new WebCamController().VideoDevices;
            if (VideoDevices.Any())
            {
                bool matchFound = false;
                foreach (var device in VideoDevices)
                {
                    if (SelectedWebcam.Equals(device.Name))
                    {
                        matchFound = true;
                        CurrentDevice = device;
                    }
                }
                if (!matchFound) CurrentDevice = VideoDevices[0];
            }
            else
            {
                //TODO send windows notification
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

                    //double ScreenRatio = Math.Round(SystemParameters.PrimaryScreenWidth / SystemParameters.PrimaryScreenHeight, 2);
                    double ScreenRatio = Math.Round((double)1920 / 1080, 2);

                    string log = "";
                    log += "Webcam Init... Screen: " + 1920 + "x" + 1080 + Environment.NewLine;
                    Collection<VideoCapabilities> SupportedVC = new Collection<VideoCapabilities>();
                    log += "Supported Video Capabilities: ";
                    foreach (var cap in _videoSource.VideoCapabilities)
                    {
                        log += cap.FrameSize.Width + "x" + cap.FrameSize.Height;
                        double tempCamRatio = Math.Round((double)cap.FrameSize.Width / cap.FrameSize.Height, 2);
                        if (tempCamRatio == ScreenRatio)
                        {
                            log += "[s]  ";
                            SupportedVC.Add(cap);
                        }
                        log += "  ";
                    }
                    log += Environment.NewLine;

                    if (SupportedVC.Count > 0)
                    {
                        VideoCapabilities SelectedVC = SupportedVC[0];
                        foreach (var cap in SupportedVC)
                        {
                            if (cap.FrameSize.Height == 720)
                            {
                                //We have decided to use 720p resolution for camera for faster processing
                                SelectedVC = cap;
                                break;
                            }
                            else if (cap.FrameSize.Width > SelectedVC.FrameSize.Width)
                            {
                                SelectedVC = cap;
                            }
                        }
                        _videoSource.VideoResolution = SelectedVC;

                    }
                    else
                    {
                        VideoCapabilities SelectedVC = _videoSource.VideoCapabilities[0];
                        foreach (var cap in _videoSource.VideoCapabilities)
                        {
                            if (cap.FrameSize.Height == 720)
                            {
                                //We have decided to use 720p resolution for camera for faster processing
                                SelectedVC = cap;
                                break;
                            }
                            else if (cap.FrameSize.Width > SelectedVC.FrameSize.Width)
                            {
                                SelectedVC = cap;
                            }
                        }
                        _videoSource.VideoResolution = SelectedVC;

                    }
                    log += "Selected Video Capability: " + _videoSource.VideoResolution.FrameSize.Width + "x" + _videoSource.VideoResolution.FrameSize.Height;
                    //JSHelperFuncs.Log(log, JSHelperFuncs.LogType.INFO);
                    _videoSource.Start();

                }
                catch (Exception e)
                {
                    //Unable to start camera
                    //TODO - show some notification?
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

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

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
