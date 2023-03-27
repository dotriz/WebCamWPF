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
using Windows.Media.MediaProperties;
using System.Collections.Generic;

namespace MCapture.Webcam
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

        }

        async private void SetupWebCam()
        {
            try
            {
                var captureManager = new MediaCapture();
                var mediaInitSettings = new MediaCaptureInitializationSettings { VideoDeviceId = CurrentDevice.Id };
                await captureManager.InitializeAsync(mediaInitSettings);

                var videoDeviceController = captureManager.VideoDeviceController;

                // Set the desired video resolution
                var resolutions = videoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);
                var resolution = resolutions.FirstOrDefault(r => (r as VideoEncodingProperties)?.Height == 720u);
                if (resolution == null) resolution = resolutions.FirstOrDefault();
                await videoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, resolution);


                var preview = new CapturePreview(captureManager);
                WebCamPlayerCircle.ImageSource = preview;
                await preview.StartAsync();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            var task = GetVideoDevicesAsync();
            task.Wait();

            StartCamera();
        }
    }
}
