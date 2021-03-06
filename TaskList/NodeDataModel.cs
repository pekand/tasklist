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
        public int parent = -1;
        public string title = null;
        public string windowTitle = null;
        public bool isExpanded = false;
        public bool isRoot = false;
        public bool isWindowsRoot = false;
        public bool isWindow = false;
        public bool isInactiveWindow = false;
        public bool isRunning = false;
        public bool isPinned = false;
        public bool isFolder = false;
        public bool isNote = false;
        public bool isDeletable = false;
        public bool isMoovable = true;
        public bool isHidden = false;
        public bool isRenamed = false;
        public bool isCurrentApp = false;
        public bool isLink = false;
        public Process process = null;
        public IntPtr handle = IntPtr.Zero;
        public int imageIndex = -1;
        public Image image = null;
        public string imageBase = null;
        public string runCommand = null;
    }
}
