using AForge.Video.DirectShow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Timers;

namespace Emgu.Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoCapture capture;
        Timer timer;

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
        bool WebCamInitialised = false;

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
                //the fps of the webcam
                int cameraFps = 30;

                //init the camera
                capture = new VideoCapture(0);

                //set the captured frame width and height (default 640x480)
                capture.Set(CapProp.FrameWidth, 1024);
                capture.Set(CapProp.FrameHeight, 768);

                //create a timer that refreshes the webcam feed
                timer = new Timer()
                {
                    Interval = 1000 / cameraFps,
                    Enabled = true
                };
                timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            }
        }

        private void StopCamera()
        {
            timer?.Stop();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            CurrentDevice = (FilterInfo)cmb.SelectedItem;
            StopCamera();
            StartCamera();
        }

        private async void timer_Tick(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var mat1 = capture.QueryFrame();
                var mat2 = new Mat();

                //flip the image horizontally
                CvInvoke.Flip(mat1, mat2, FlipType.Horizontal);

                //convert the mat to a bitmap
                var bmp = mat2.ToImage<Bgr, byte>().ToBitmap();

                //copy the bitmap to a memorystream
                var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Bmp);

                //display the image on the ui
                WebCamPlayerCircle.ImageSource = BitmapFrame.Create(ms);
            });
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
