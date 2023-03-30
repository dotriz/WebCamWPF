using AForge.Controls;
using AForge.Video.DirectShow;
using System.Windows.Forms.Integration;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AForge.Video;

namespace AForge.Webcam.Control
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


        #endregion

        public void WebCamSetup()
        {
            SelectedWebcam = "";
            this.DataContext = this;
            GetVideoDevices();

        }

        private void GetVideoDevices()
        {
            VideoDevices = new WebCamController().VideoDevices;
            if (VideoDevices.Any()) CurrentDevice = VideoDevices[0];
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                try
                {
                    _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);

                    VideoCapabilities SelectedVC = _videoSource.VideoCapabilities[0];
                    foreach (var cap in _videoSource.VideoCapabilities)
                    {
                        if (cap.FrameSize.Height == 720)
                        {
                            SelectedVC = cap;
                            break;
                        }
                    }
                    _videoSource.VideoResolution = SelectedVC;


                    var videoPlayer = new VideoSourcePlayer
                    {
                        VideoSource = _videoSource,
                        BorderColor = System.Drawing.Color.Transparent,
                        // Want to hide connectin etc text message, so making it transparent
                        ForeColor = System.Drawing.Color.Transparent
                    };
                    videoPlayer.Start();

                    windowsFormsHost.Child = videoPlayer;
                }
                catch
                {

                }
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;
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
