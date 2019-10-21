﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList
{
    class NodeDataModel
    {
        public int id = 0;
        public int parent = 0;
        public bool isRoot = false;
        public bool isWindow = false;
        public bool isFolder = false;
        public bool isDeletable = false;
        public bool isMoovable = true;
        public bool isHidden = false;
        public bool isRenamed = false;
        public bool isCurrentApp = false;
        public string name = "";
        public Process process = null;
        public IntPtr handle = IntPtr.Zero;
        public int imageIndex = -1;
        public Bitmap image = null;
    }
}
