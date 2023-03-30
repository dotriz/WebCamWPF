using AForge.Video.DirectShow;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OpenCv.Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private VideoCapture capCamera;

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

        private void PlayCamera()
        {
            while (!capCamera.IsDisposed)
            {
                Mat matImage = new Mat();
                capCamera.Read(matImage); // same as cvQueryFrame
                if (matImage.Empty()) break;
                //Thread.Sleep(sleepTime);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var converted = Convert(BitmapConverter.ToBitmap(matImage));
                    WebCamPlayerCircle.ImageSource = converted;
                }));
            }
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                capCamera = new VideoCapture(0);
                var x = capCamera.Get(VideoCaptureProperties.FrameWidth);
                // set the camera resolution to 720p
                capCamera.Set(VideoCaptureProperties.FrameWidth, 1280);
                capCamera.Set(VideoCaptureProperties.FrameHeight, 720);

                new Thread(PlayCamera).Start();
            }
        }

        private void StopCamera()
        {
            if (capCamera != null && capCamera.IsOpened())
            {
                capCamera.Dispose();
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
