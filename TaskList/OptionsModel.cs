using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList
{
    public class OptionsModel
    {
        public System.Drawing.Point Location = new System.Drawing.Point(100, 100);
        public System.Drawing.Size Size = new System.Drawing.Size(500, 500);
        public bool Maximised = false;
        public bool Minimised = false;
        public bool firstRun = true;
        public bool mostTop = false;
        public string font = null;
    }
}
