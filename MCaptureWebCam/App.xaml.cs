using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MCapture.Webcam
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string data = "Crash Log: " + e.Exception.StackTrace + " - " + e.Exception.Message;
            JSLog.Log(data, JSLog.LogType.CRASH);
        }
    }

    
}
