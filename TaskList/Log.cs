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

        public static void write(string text = "") {
#if DEBUG
            File.AppendAllText(@"log.txt", text + Environment.NewLine);
#endif
        }

        public static void clear()
        {
#if DEBUG
            File.WriteAllText(@"log.txt", "");
#endif
        }
    }
}
