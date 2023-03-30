using AForge.Video.DirectShow;
using System.Collections.ObjectModel;

namespace AForge.Webcam.Control
{
    public class WebCamController
    {
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public static WebCamController Instance { get; } = new WebCamController();
        static WebCamController() { }
        public WebCamController()
        {
            LoadWebCams();
        }

        internal void LoadWebCams()
        {
            //if(Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1) // windows 7
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
        }

        public void Dispose()
        {

        }
    }
}
