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

namespace Test.Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        public void WebCamSetup()
        {
            SelectedWebcam = "";
            this.DataContext = this;
            GetVideoDevices();

            var videoDevice = new VideoCaptureDevice(); // Create a new VideoCaptureDevice object
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice); // Get a list of available video devices

            // Select the first available video device (you can customize this based on your requirements)
            if (videoDevices.Count > 0)
            {
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
            }

            VideoCapabilities SelectedVC = videoDevice.VideoCapabilities[0];
            foreach (var cap in videoDevice.VideoCapabilities)
            {
                if (cap.FrameSize.Height == 720)
                {
                    //We have decided to use 720p resolution for camera for faster processing
                    SelectedVC = cap;
                    break;
                }
            }
            videoDevice.VideoResolution = SelectedVC;


            var videoPlayer = new VideoSourcePlayer();
            videoPlayer.VideoSource = videoDevice;
            videoPlayer.Start();

            windowsFormsHost.Child = videoPlayer;

            //grid.Children.Add(windowsFormsHost); // Add the WindowsFormsHost control to the grid

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
                
            }
        }

        private void StopCamera()
        {
            
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
