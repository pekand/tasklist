using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList
{
    public static class Log
    {
#if DEBUG
        private static string lastMessage = null;
        public static bool directoryExists = false;
        public static string logFileName = String.Format("log\\{0}-log.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
#endif

        //LOG_WRITE
        public static void write(string text = "") {
#if DEBUG
            if (text == lastMessage) { // remove duplicite messages
                return;
            }

            if (!directoryExists && !Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
                directoryExists = true;
            }

            lastMessage = text;
            File.AppendAllText(logFileName, text + Environment.NewLine);
#endif
        }
    }
}
