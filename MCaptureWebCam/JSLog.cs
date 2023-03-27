using System;
using System.IO;

namespace MCapture.Webcam
{
    public static class JSLog
    {
        public static bool DEV_MODE = false;

        public enum LogType
        {
            FFMPEG = 0,
            INFO = 1,
            CRASH = 2,
            EXCEPTION = 4
        }

        public enum LogLevel
        {
            GENERAL = 0,
            DEBUG = 1,
            TRACE = 2
        }

        public static string GetLogFolder()
        {
            string logFolder = Directory.GetCurrentDirectory() + "\\log";
            if (!Directory.Exists(logFolder))
            {
                try
                {
                    Directory.CreateDirectory(logFolder);
                }
                catch
                {
                    logFolder = Directory.GetCurrentDirectory();
                }
            }

            return logFolder;

        }

        public static string GetLogFilePath()
        {
            string date = DateTime.Now.ToString("yy-MM");
            string logpath = GetLogFolder() + "\\jumpshare.log." + date + ".txt";
            if (!File.Exists(logpath))
            {
                try
                {
                    File.AppendAllText(logpath, DateTime.Now.ToString("G") + " - First log of the day" + Environment.NewLine);
                }
                catch
                {
                    logpath = GetLogFolder() + "\\jumpshare.log.txt";
                }
            }

            return logpath;
        }

        public static void Log(string data, LogType type = LogType.INFO, LogLevel level = LogLevel.GENERAL)
        {
            try
            {
                bool DebugMode = false;
#if DEBUG
                DebugMode = true;
#endif
                if (DEV_MODE) DebugMode = true;

                if (level == LogLevel.TRACE && true) return;
                if (level == LogLevel.DEBUG && !DebugMode) return;

                if (type == LogType.FFMPEG) data = DebugMode ? data : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));

                File.AppendAllText(GetLogFilePath(), DateTime.Now.ToString("G") + " - " +
                    level.ToString() + " - " +
                    type.ToString() + " - " +
                    data + Environment.NewLine);
            }
            catch { }
        }
    }
}
