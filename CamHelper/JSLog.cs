using System;
using System.IO;

namespace CamHelper
{
    public static class JSLog
    {

        public enum LogType
        {
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
            string logFolder = AppContext.BaseDirectory + @"log";
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
            string logpath = GetLogFolder() + @"\logs." + date + ".txt";
            if (!File.Exists(logpath))
            {
                try
                {
                    File.AppendAllText(logpath, DateTime.Now.ToString("G") + " - First log of the day" + Environment.NewLine);
                }
                catch
                {
                    logpath = GetLogFolder() + @"\logs.txt";
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

                if (level == LogLevel.DEBUG && !DebugMode) return;

                File.AppendAllText(GetLogFilePath(), DateTime.Now.ToString("G") + " - " +
                    level.ToString() + " - " +
                    type.ToString() + " - " +
                    data + Environment.NewLine);
            }
            catch { }
        }
    }
}
