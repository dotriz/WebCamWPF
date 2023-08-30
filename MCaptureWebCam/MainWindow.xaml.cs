using MediaCaptureWPF;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using System.Linq;
using Windows.Media.MediaProperties;
using System.Windows.Media;
using CamHelper;

namespace MCapture.Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaCapture captureManager;
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
                captureManager = new MediaCapture();
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
                WebCamPlayerCircleContainer.RenderTransform = new ScaleTransform(-1, 1);
                await preview.StartAsync();

                var WebCamPixelWidth = (int)((VideoEncodingProperties)resolution).Width;
                var WebCamPixelHeight = (int)((VideoEncodingProperties)resolution).Height;
                JSLog.Log("Selected Media Capture Camera Resolution: " + WebCamPixelWidth + "x" + WebCamPixelHeight, JSLog.LogType.INFO);

            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StartCamera()
        {
            SetupWebCam();
        }

        private async void StopCamera()
        {
            if (captureManager != null)
            {
                try
                {
                    await captureManager.StopPreviewAsync();
                }
                catch { }
            }
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

            return null;

        }

        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            var task = GetVideoDevicesAsync();
            task.Wait();

            btnStartCamera.IsEnabled = false;
        }
    }
}
