using MediaCaptureWPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MCapture.Webcam2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<DeviceInformation> VideoDevices { get; set; }
        public DeviceInformation CurrentDevice
        {
            get; set;
        }

        public MainWindow()
        {
            InitializeComponent();

            var task = GetVideoDevicesAsync();
            task.Wait();

        }

        async private void SetupWebCam()
        {
            var captureManager = new MediaCapture();
            var mediaInitSettings = new MediaCaptureInitializationSettings { VideoDeviceId = CurrentDevice.Id };
            await captureManager.InitializeAsync(mediaInitSettings);

            var preview = new CapturePreview(captureManager);
            this.WebCamPlayerCircle.ImageSource = preview;
            await preview.StartAsync();
        }

        private void StartCamera()
        {
            SetupWebCam();
        }

        private void StopCamera()
        {
            //
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;
            CurrentDevice = (DeviceInformation)cmb.SelectedItem;
            StopCamera();
            StartCamera();
        }
        
        public async Task<string> GetVideoDevicesAsync()
        {
            VideoDevices = new ObservableCollection<DeviceInformation>();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            foreach (DeviceInformation device in devices)
            {
                //if (MediaCapture.IsVideoProfileSupported(device.Id))
                {
                    VideoDevices.Add(device);
                }
            }

            CurrentDevice = VideoDevices.FirstOrDefault();
            comboBox.ItemsSource = VideoDevices;
            comboBox.SelectedItem = CurrentDevice;

            //if (VideoDevices.Any())
            //{
            //    bool matchFound = false;
            //    foreach (var device in VideoDevices)
            //    {
            //        if (SelectedWebcam.Equals(device.Name))
            //        {
            //            matchFound = true;
            //            CurrentDevice = device;
            //        }
            //    }
            //    if (!matchFound) CurrentDevice = VideoDevices[0];
            //}
            //else
            //{
            //    //TODO send windows notification
            //    //MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            return null;

        }

    }
}
