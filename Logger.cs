using System;
using System.IO;

namespace AzureIOTTrackerSimulator
{
    public static class Logger
    {
        private static StreamWriter writer;
        static Logger()
        {
            writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\iot.log");
        }

        public static void Log(string message)
        {
            writer.WriteLine(DateTime.Now.ToLongTimeString() + "; " + message);
            writer.Flush();
        }
    }
}
