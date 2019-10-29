using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace TaskList
{
    public partial class TaskListForm : Form
    {
        public OptionsModel options = new OptionsModel();

        TreeNode rootNode = null;
        TreeNode windowsRootNode = null;

        List<TreeNode> allNodes = new List<TreeNode>();
        List<TreeNode> allWindowsNodes = new List<TreeNode>();
        List<TreeNode> allInactiveWindowsNodes = new List<TreeNode>();
        List<TreeNode> allFolderNodes = new List<TreeNode>();
        List<TreeNode> allNoteNodes = new List<TreeNode>();
        List<TreeNode> allLinkNodes = new List<TreeNode>();

        ImageList imageList = new ImageList();
        int lastNodeIndex = 0;
        int lastImageIndex = 0;
        int defaultIconIndex = 0;
        int folderIconIndex = 0;
        int noteIconIndex = 0;
        int systemFolderIconIndex = 0;


        public Color rootColor = Color.FromArgb(0, 0, 0);
        public Color windowsFolderColor = Color.FromArgb(0, 0, 0);
        public Color directoryLinkColor = Color.FromArgb(0, 82, 204);
        public Color linkColor = Color.FromArgb(0, 82, 204);
        public Color windowColor = Color.FromArgb(0, 0, 0);
        public Color inactiveWindowColor = Color.FromArgb(128, 128, 128);
        public Color folderColor = Color.FromArgb(0, 0, 0);
        public Color noteColor = Color.FromArgb(0, 0, 0);


        /* FORM EVENTS */

        public TaskListForm()
        {

            Log.write("Constructor");
            InitializeComponent();
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            Log.write("Load");

#if DEBUG
            this.Text += " - DEBUG";
#endif

            

            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            treeView.ImageList = this.imageList;

            Image defaultIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.TaskList.ico"));
            this.defaultIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.defaultIconIndex, defaultIcon);

            Image folderIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.folder.ico"));
            this.folderIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.folderIconIndex, folderIcon);

            Image noteIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.note.ico"));
            this.noteIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.noteIconIndex, noteIcon);

            Image systemfolderIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.systemfolder.ico"));
            this.systemFolderIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.systemFolderIconIndex, systemfolderIcon);



            this.restoreFormSettings();

            this.pairInactiveWindowsToNodes();

            if (this.rootNode == null) {
                var rootNodeData = new NodeDataModel();
                rootNodeData.id = ++this.lastNodeIndex;
                rootNodeData.parent = 0;
                rootNodeData.title = "Tasks";
                rootNodeData.isRoot = true;
                rootNodeData.isMoovable = false;
                rootNodeData.image = defaultIcon;
                this.rootNode = new TreeNode();
                this.rootNode.Text = "Tasks";
                this.rootNode.ImageIndex = this.defaultIconIndex;
                this.rootNode.SelectedImageIndex = this.defaultIconIndex;
                this.rootNode.Tag = rootNodeData;
            }

            if (this.windowsRootNode == null)
            {
                var windowsNodeData = new NodeDataModel();
                windowsNodeData.id = ++this.lastNodeIndex;
                windowsNodeData.parent = ((NodeDataModel)this.rootNode.Tag).id;
                windowsNodeData.title = "Windows";
                windowsNodeData.isMoovable = false;
                windowsNodeData.isWindowsRoot = true;
                this.windowsRootNode = new TreeNode();
                this.windowsRootNode.Text = "Windows";
                this.windowsRootNode.ImageIndex = this.defaultIconIndex;
                this.windowsRootNode.SelectedImageIndex = this.defaultIconIndex;
                this.windowsRootNode.Tag = windowsNodeData;
                this.rootNode.Nodes.Add(this.windowsRootNode);
            }

            this.updateTree();

            treeView.BeginUpdate();

            if (rootNode != null) {
                treeView.Nodes.Add(rootNode);
            }
            rootNode.Expand();
            windowsRootNode.Expand();

            treeView.EndUpdate();

            autorunToolStripMenuItem.Checked = SystemManager.isAutorunSet();


            this.updateTimer.Enabled = true;
        }

        private void TaskList_Shown(object sender, EventArgs e)
        {
            Log.write("Show");
        }

        private void TaskList_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.write("Closing");
            this.saveFormSettings();            
        }

        /* UPDATE */
        private void update_Tick(object sender, EventArgs e)
        {
            Log.write("Update tick");
            this.updateTree();
        }

        private void updateTree()
        {
            Log.write("updatTree");

            List<TreeNode> toRemoveNodes = new List<TreeNode>();

            // windows add 
            List<WindowData> windowsList = TaskManager.GetOpenWindows();


            if (this.allInactiveWindowsNodes.Count > 0)
            {
                this.pairInactiveWindowsToNodes(windowsList, true);
            }

            foreach (WindowData window in windowsList)
            {

                //skip current app
                if (this.Handle == window.handle)
                {
                    continue;
                }

                bool exists = false;
                foreach (TreeNode oldNode in this.allWindowsNodes)
                {
                    var oldNodeData = (NodeDataModel)oldNode.Tag;

                    if (oldNodeData.handle == window.handle)
                    {

                        string windowTitle = TaskManager.getWindowTitle(window.handle);
                        if (windowTitle != "" && windowTitle != oldNodeData.windowTitle)
                        {
                            if (!oldNodeData.isRenamed)
                            {
                                oldNodeData.title = windowTitle;
                                oldNode.Text = windowTitle;
                            }

                            oldNodeData.windowTitle = windowTitle;
                        }


                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                //skip current app
                if (window.handle == this.Handle)
                {
                    continue;
                }

                this.CreateNode(
                    window.handle,
                    "Window",
                    window.path,
                    windowsRootNode,
                    false,
                    true,
                    false,
                    false,
                    false
               );
            }

            // find old nodes
            foreach (TreeNode oldNode in this.allWindowsNodes)
            {

                NodeDataModel nodeData = (NodeDataModel)oldNode.Tag;
                bool exists = false;
                foreach (WindowData window in windowsList)
                {
                    if (nodeData.handle == window.handle)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    toRemoveNodes.Add(oldNode);
                }
            }

            // remove old nodes
            foreach (TreeNode oldNode in toRemoveNodes)
            {
                NodeDataModel oldNodeData = (NodeDataModel)oldNode.Tag;
                if (oldNodeData.isPinned)
                {
                    oldNodeData.isWindow = false;
                    oldNodeData.isInactiveWindow = true;
                    oldNodeData.handle = IntPtr.Zero;
                    oldNode.ForeColor = inactiveWindowColor;
                    this.allWindowsNodes.Remove(oldNode);
                    this.allInactiveWindowsNodes.Add(oldNode);
                }
                else
                {
                    this.RemoveNode(oldNode);
                }
            }
        }

        public void pairInactiveWindowsToNodes(List<WindowData> windowsList = null, bool skipProcessSearch = false)
        {
            Log.write("pairInactiveWindowsToNodes");

            // map window by window title
            if (windowsList == null)
            {
                windowsList = TaskManager.GetOpenWindows();
            }

            List<TreeNode> moveToWindowsNodes = new List<TreeNode>();

            foreach (WindowData windowData in windowsList)
            {
                try
                {
                    windowData.title = TaskManager.getWindowTitle(windowData.handle);
                    windowData.image = TaskManager.GetSmallWindowIcon(windowData.handle);
                    windowData.imageBase = ImageManager.ImageToString(windowData.image);
                    windowData.process = TaskManager.getProcessFromHandle(windowData.handle);
                    windowData.path = windowData.process != null ? windowData.process.MainModule.FileName : null;
                }
                catch (Exception e)
                {
                    Log.write(e.Message);
                }
            }

            // search by title
            foreach (WindowData windowData in windowsList)
            {
                foreach (TreeNode inactiveNode in allInactiveWindowsNodes)
                {
                    NodeDataModel nodeData = (NodeDataModel)inactiveNode.Tag;

                    if (nodeData.handle != IntPtr.Zero)
                    {
                        continue;
                    }

                    if (nodeData.windowTitle == windowData.title)
                    {

                        nodeData.handle = windowData.handle;
                        nodeData.isWindow = true;
                        nodeData.isInactiveWindow = false;
                        nodeData.runCommand = windowData.path;
                        inactiveNode.ForeColor = windowColor;
                        moveToWindowsNodes.Add(inactiveNode);
                        break;
                    }
                }
            }

            //search by process path
            foreach (WindowData windowData in windowsList)
            {
                foreach (TreeNode inactiveNode in allInactiveWindowsNodes)
                {
                    NodeDataModel nodeData = (NodeDataModel)inactiveNode.Tag;

                    if (nodeData.handle != IntPtr.Zero)
                    {
                        continue;
                    }

                    if (nodeData.runCommand == windowData.path)
                    {

                        nodeData.handle = windowData.handle;
                        nodeData.isWindow = true;
                        nodeData.isInactiveWindow = false;
                        inactiveNode.ForeColor = windowColor;
                        moveToWindowsNodes.Add(inactiveNode);
                        break;
                    }
                }
            }

            // search by icon
            foreach (WindowData windowData in windowsList)
            {
                foreach (TreeNode inactiveNode in allInactiveWindowsNodes)
                {
                    NodeDataModel nodeData = (NodeDataModel)inactiveNode.Tag;

                    if (nodeData.handle != IntPtr.Zero)
                    {
                        continue;
                    }

                    if (nodeData.imageBase != null && nodeData.imageBase != "" && windowData.imageBase != null && windowData.imageBase != "" && nodeData.imageBase == windowData.imageBase)
                    {

                        nodeData.handle = windowData.handle;
                        nodeData.isWindow = true;
                        nodeData.isInactiveWindow = false;
                        inactiveNode.ForeColor = windowColor;
                        moveToWindowsNodes.Add(inactiveNode);
                        break;
                    }
                }
            }

            if (!skipProcessSearch)
            {
                TaskManager.FindProcessByWithWindowHandle(windowsList);

                //map window by parent process
                foreach (WindowData windowData in windowsList)
                {

                    try
                    {
                        if (windowData.process == null)
                        {
                            continue;
                        }

                        string processPath = windowData.process.MainModule.FileName;

                        foreach (TreeNode inactiveNode in allInactiveWindowsNodes)
                        {
                            NodeDataModel nodeData = (NodeDataModel)inactiveNode.Tag;

                            if (nodeData.handle != IntPtr.Zero)
                            {
                                continue;
                            }

                            if (nodeData.runCommand == processPath)
                            {
                                nodeData.handle = windowData.handle;
                                nodeData.isWindow = true;
                                nodeData.isInactiveWindow = false;
                                inactiveNode.ForeColor = windowColor;
                                moveToWindowsNodes.Add(inactiveNode);
                                break;
                            }
                        }

                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        Log.write(e.Message);
                    }
                    catch (System.InvalidOperationException e)
                    {
                        Log.write(e.Message);
                    }
                }
            }            
            
            // move nodes from inactive to active windows
            foreach (TreeNode node in moveToWindowsNodes)
            {
                allInactiveWindowsNodes.Remove(node);
                allWindowsNodes.Add(node);
            }
        }

        /* SAVE */

        public void saveFormSettings()
        {
            Log.write("FormManager saveFormPosition");

            if (this.WindowState == FormWindowState.Maximized)
            {
                this.options.Location = this.RestoreBounds.Location;
                this.options.Size = this.RestoreBounds.Size;
                this.options.Maximised = true;
                this.options.Minimised = false;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.options.Location = this.Location;
                this.options.Size = this.Size;
                this.options.Maximised = false;
                this.options.Minimised = false;
            }
            else
            {
                this.options.Location = this.RestoreBounds.Location;
                this.options.Size = this.RestoreBounds.Size;
                this.options.Maximised = false;
                this.options.Minimised = true;
            }

            this.options.mostTop = this.TopMost;
            this.options.font = TypeDescriptor.GetConverter(typeof(Font)).ConvertToInvariantString(this.treeView.Font);

#if DEBUG
            string settingsFilePath = "settings.xml";
#else
            
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TaskList\\";
            if (!Directory.Exists(roamingPath))
            {
                Directory.CreateDirectory(roamingPath);
            }

            string settingsFilePath = roamingPath + "settings.xml";    
#endif
            XElement options = new XElement("options");
            options.Add(new XElement("LocationX", this.options.Location.X));
            options.Add(new XElement("LocationY", this.options.Location.Y));
            options.Add(new XElement("SizeWidth", this.options.Size.Width));
            options.Add(new XElement("SizeHeight", this.options.Size.Height));
            options.Add(new XElement("Maximised", this.options.Maximised ? "1" : "0"));
            options.Add(new XElement("Maximised", this.options.Minimised ? "1" : "0"));
            options.Add(new XElement("FirstRun", this.options.firstRun ? "1" : "0"));
            options.Add(new XElement("MostTop", this.options.mostTop ? "1" : "0"));
            options.Add(new XElement("Font", this.options.font));
            options.Add(new XElement("RememberState", this.options.rememberState ? "1" : "0"));
            options.Add(new XElement("ShowInTaskbar", this.options.ShowInTaskbar ? "1" : "0"));

            XElement nodes = new XElement("Nodes");
            options.Add(nodes);

            List<TreeNode> allSortedNodes = new List<TreeNode>();

            this.getNodes(allSortedNodes, rootNode);

            foreach (TreeNode node in allSortedNodes)
            {
                XElement nodeElement = new XElement("Node");

                NodeDataModel nodeData = (NodeDataModel)node.Tag;

                if (nodeData.isWindow)
                {
                    nodeData.isWindow = false;
                    nodeData.isInactiveWindow = true;
                }

                nodeElement.Add(new XElement("id", nodeData.id));
                nodeElement.Add(new XElement("title", nodeData.title));
                nodeElement.Add(new XElement("windowTitle", nodeData.windowTitle));
                nodeElement.Add(new XElement("parent", nodeData.parent));
                nodeElement.Add(new XElement("isExpanded", nodeData.isExpanded ? "1" : "0"));
                nodeElement.Add(new XElement("isRoot", nodeData.isRoot ? "1" : "0"));
                nodeElement.Add(new XElement("isWindowsRoot", nodeData.isWindowsRoot ? "1" : "0"));
                nodeElement.Add(new XElement("isWindow", nodeData.isWindow ? "1" : "0"));
                nodeElement.Add(new XElement("isInactiveWindow", nodeData.isInactiveWindow ? "1" : "0"));
                nodeElement.Add(new XElement("isPinned", nodeData.isPinned ? "1" : "0"));
                nodeElement.Add(new XElement("isFolder", nodeData.isFolder ? "1" : "0"));
                nodeElement.Add(new XElement("isNote", nodeData.isNote ? "1" : "0"));
                nodeElement.Add(new XElement("isLink", nodeData.isLink ? "1" : "0"));
                nodeElement.Add(new XElement("isDeletable", nodeData.isDeletable ? "1" : "0"));
                nodeElement.Add(new XElement("isMoovable", nodeData.isMoovable ? "1" : "0"));
                nodeElement.Add(new XElement("isHidden", nodeData.isHidden ? "1" : "0"));
                nodeElement.Add(new XElement("isRenamed", nodeData.isRenamed ? "1" : "0"));
                nodeElement.Add(new XElement("isCurrentApp", nodeData.isCurrentApp ? "1" : "0"));
                nodeElement.Add(new XElement("runCommand", nodeData.runCommand));

                if ((nodeData.isInactiveWindow || nodeData.isLink) && nodeData.image != null)
                {
                    nodeElement.Add(new XElement("image", ImageManager.ImageToString(nodeData.image)));
                }

                nodes.Add(nodeElement);
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(settingsFilePath);
            file.Write(options);
            file.Close();
        }

        /* LOAD */

        public void restoreFormSettings()
        {
            Log.write("FormManager restoreFormPosition");

#if DEBUG
            string settingsFilePath = "settings.xml";
#else
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TaskList\\";
            string settingsFilePath = roamingPath + "settings.xml";
#endif

            if (!File.Exists(settingsFilePath))
            {
                return;
            }

            XmlReaderSettings xws = new XmlReaderSettings
            {
                CheckCharacters = false
            };

            string xml = "";

            try
            {
                using (StreamReader streamReader = new StreamReader(settingsFilePath, Encoding.UTF8))
                {
                    xml = streamReader.ReadToEnd();
                }
            }
            catch (System.IO.IOException ex)
            {
                Log.write(ex.Message);
            }

            int lastNodeIndex = 0;

            try
            {
                using (XmlReader xr = XmlReader.Create(new StringReader(xml), xws))
                {

                    XElement root = XElement.Load(xr);

                    foreach (XElement option in root.Elements())
                    {
                        if (option.Name.ToString() == "LocationX")
                        {
                            this.options.Location.X = Int32.Parse(option.Value);
                        }

                        if (option.Name.ToString() == "LocationY")
                        {
                            this.options.Location.Y = Int32.Parse(option.Value);
                        }

                        if (option.Name.ToString() == "SizeWidth")
                        {
                            this.options.Size.Width = Int32.Parse(option.Value);
                        }

                        if (option.Name.ToString() == "SizeHeight")
                        {
                            this.options.Size.Height = Int32.Parse(option.Value);
                        }

                        if (option.Name.ToString() == "Maximised")
                        {
                            this.options.Maximised = option.Value == "1";
                        }

                        if (option.Name.ToString() == "Maximised")
                        {
                            this.options.Minimised = option.Value == "1";
                        }

                        if (option.Name.ToString() == "FirstRun")
                        {
                            this.options.firstRun = option.Value == "1";
                        }

                        if (option.Name.ToString() == "MostTop")
                        {
                            this.options.mostTop = option.Value == "1";
                        }

                        if (option.Name.ToString() == "Font")
                        {
                            this.options.font = option.Value;
                        }

                        if (option.Name.ToString() == "RememberState")
                        {
                            this.options.rememberState = option.Value == "1";
                        }

                        if (option.Name.ToString() == "ShowInTaskbar")
                        {
                            this.options.ShowInTaskbar = option.Value == "1";
                        }

                        if (option.Name.ToString() == "Nodes")
                        {
                            foreach (XElement node in option.Elements())
                            {
                                if (node.Name.ToString() == "Node")
                                {
                                    TreeNode newNode = new TreeNode();
                                    NodeDataModel newNodeData = new NodeDataModel();
                                    newNode.Tag = newNodeData;
                                    allNodes.Add(newNode);

                                    foreach (XElement attribute in node.Elements())
                                    {
                                        if (attribute.Name.ToString() == "id")
                                        {
                                            newNodeData.id = Int32.Parse(attribute.Value);
                                            if (lastNodeIndex < newNodeData.id)
                                            {
                                                lastNodeIndex = newNodeData.id;
                                            }
                                        }

                                        if (attribute.Name.ToString() == "title")
                                        {
                                            newNodeData.title = attribute.Value;
                                        }

                                        if (attribute.Name.ToString() == "windowTitle")
                                        {
                                            newNodeData.windowTitle = attribute.Value;
                                        }

                                        if (attribute.Name.ToString() == "parent")
                                        {
                                            newNodeData.parent = Int32.Parse(attribute.Value);
                                        }

                                        if (attribute.Name.ToString() == "isExpanded")
                                        {
                                            newNodeData.isExpanded = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isRoot")
                                        {
                                            newNodeData.isRoot = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isPinned")
                                        {
                                            newNodeData.isPinned = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isInactiveWindow")
                                        {
                                            newNodeData.isInactiveWindow = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isWindowsRoot")
                                        {
                                            newNodeData.isWindowsRoot = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isFolder")
                                        {
                                            newNodeData.isFolder = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isNote")
                                        {
                                            newNodeData.isNote = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isLink")
                                        {
                                            newNodeData.isLink = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isDeletable")
                                        {
                                            newNodeData.isDeletable = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isMoovable")
                                        {
                                            newNodeData.isMoovable = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isHidden")
                                        {
                                            newNodeData.isHidden = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isRenamed")
                                        {
                                            newNodeData.isRenamed = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isCurrentApp")
                                        {
                                            newNodeData.isCurrentApp = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "image")
                                        {
                                            newNodeData.imageBase = attribute.Value;

                                            if (newNodeData.imageBase != "")
                                            {
                                                try
                                                {
                                                    newNodeData.image = (Bitmap)ImageManager.StringToImage(newNodeData.imageBase);

                                                    this.imageList.Images.Add("image" + (this.lastImageIndex), newNodeData.image);
                                                    newNode.ImageIndex = this.lastImageIndex;
                                                    newNode.SelectedImageIndex = this.lastImageIndex;
                                                    lastImageIndex++;
                                                }
                                                catch (Exception e)
                                                {
                                                    Log.write(e.Message);
                                                }
                                            }
                                        }

                                        if (attribute.Name.ToString() == "runCommand")
                                        {
                                            newNodeData.runCommand = attribute.Value;
                                        }
                                    }

                                    if (newNodeData.isRoot)
                                    {
                                        newNode.ForeColor = rootColor;
                                        this.rootNode = newNode;
                                    }

                                    if (newNodeData.isWindowsRoot)
                                    {
                                        newNode.ForeColor = windowsFolderColor;
                                        this.windowsRootNode = newNode;
                                    }

                                    if (newNodeData.isFolder)
                                    {
                                        newNode.ForeColor = folderColor;
                                        newNode.ImageIndex = this.folderIconIndex;
                                        newNode.SelectedImageIndex = this.folderIconIndex;
                                        allFolderNodes.Add(newNode);
                                    }

                                    if (newNodeData.isWindow)
                                    {
                                        if (newNodeData.isRenamed)
                                        {
                                            newNode.Text = newNodeData.title;
                                        }
                                        else
                                        {
                                            newNode.Text = newNodeData.windowTitle;
                                        }

                                        newNode.ForeColor = windowColor;
                                    }
                                    else
                                    {
                                        newNode.Text = newNodeData.title;
                                    }

                                    if (newNodeData.isNote)
                                    {
                                        newNode.ForeColor = noteColor;
                                        newNode.ImageIndex = this.noteIconIndex;
                                        newNode.SelectedImageIndex = this.noteIconIndex;
                                        allNoteNodes.Add(newNode);
                                    }

                                    if (newNodeData.isLink)
                                    {
                                        if (newNodeData.image == null) {
                                            newNode.ImageIndex = this.systemFolderIconIndex;
                                            newNode.SelectedImageIndex = this.systemFolderIconIndex;
                                        }

                                        if (File.Exists(newNodeData.runCommand))
                                        {
                                            newNode.ForeColor = linkColor;
                                        }

                                        if (Directory.Exists(newNodeData.runCommand))
                                        {
                                            newNode.ForeColor = directoryLinkColor;
                                        }

                                        allLinkNodes.Add(newNode);
                                    }

                                    if (newNodeData.isInactiveWindow)
                                    {
                                        newNode.ForeColor = inactiveWindowColor;
                                        allInactiveWindowsNodes.Add(newNode);
                                    }
                                }
                            }
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                Log.write(ex.Message);
            }

            showInTaskbarToolStripMenuItem.Checked = this.options.ShowInTaskbar;

            if (!this.options.ShowInTaskbar)
            {
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else
            {
                this.ShowInTaskbar = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }

            if (this.rootNode != null)
            {
                this.restoreNodes(allNodes, this.rootNode);

                // expand nodes
                foreach (TreeNode node in allNodes)
                {
                    NodeDataModel nodeData = (NodeDataModel)node.Tag;
                    if (nodeData.isExpanded)
                    {
                        node.Expand();
                    }
                    else {
                        node.Collapse();
                    }
                }

                this.lastNodeIndex = lastNodeIndex;
            }

            if (this.options.firstRun)
            {
                options.firstRun = false;
            }
            else if (this.options.Maximised)
            {
                this.WindowState = FormWindowState.Maximized;
                this.Location = options.Location;
                this.Size = options.Size;
            }
            else if (this.options.Minimised)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Location = options.Location;
                this.Size = options.Size;
            }
            else
            {
                this.Location = options.Location;
                this.Size = options.Size;
            }

            if (FormManager.IsOnScreen(this) == -1)
            {
                this.WindowState = FormWindowState.Normal;
                this.Left = 100;
                this.Top = 100;
                this.Width = 500;
                this.Height = 500;
            }

            this.setTopMost(options.mostTop);

            rememberToolStripMenuItem.Checked = options.rememberState;

            if (this.options.font != null)
            {
                this.treeView.Font = TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(this.options.font) as Font;
            }
        }

        
        /* NODES */

        public TreeNode CreateNode(IntPtr handle, string title = null, string runCommand = null, TreeNode parent = null, bool isFolder = false, bool isWindow = false, bool isInactiveWindow = false, bool isNote = false, bool isLink = false)
        {
            Log.write("Create Node");
            TreeNode node = new TreeNode();
            NodeDataModel nodeData = new NodeDataModel();
            node.Tag = nodeData;

            nodeData.id = ++this.lastNodeIndex;

            if (title != null) {
                node.Text = title;
                nodeData.title = title;
            }

            if (isFolder) {
                node.ForeColor = folderColor;
                nodeData.isFolder = true;
                nodeData.imageIndex = this.folderIconIndex;
                node.ImageIndex = this.folderIconIndex;
                node.SelectedImageIndex = this.folderIconIndex;
                allFolderNodes.Add(node);
            } else
            if (isNote)
            {
                node.ForeColor = noteColor;
                nodeData.isNote = true;
                nodeData.imageIndex = this.noteIconIndex;
                node.ImageIndex = this.noteIconIndex;
                node.SelectedImageIndex = this.noteIconIndex;
                allNoteNodes.Add(node);
            }
            else
            if (isLink)
            {
                nodeData.isLink = true;
                nodeData.runCommand = runCommand;

                try
                {
                    if (Directory.Exists(runCommand))
                    {
                        nodeData.imageIndex = systemFolderIconIndex;
                        node.ImageIndex = nodeData.imageIndex;
                        node.SelectedImageIndex = nodeData.imageIndex;
                        node.ForeColor = directoryLinkColor;
                    }

                    if (File.Exists(runCommand)) {
                        nodeData.image = System.Drawing.Icon.ExtractAssociatedIcon(runCommand).ToBitmap(); ;
                        nodeData.imageBase = ImageManager.ImageToString(nodeData.image);
                        this.imageList.Images.Add("image" + this.lastImageIndex, (Image)nodeData.image);
                        nodeData.imageIndex = this.lastImageIndex++;
                        node.ImageIndex = nodeData.imageIndex;
                        node.SelectedImageIndex = nodeData.imageIndex;
                        node.ForeColor = linkColor;
                    }
                } catch (Exception e) {
                    Log.write(e.Message);
                }

                allNoteNodes.Add(node);
            }
            else
            if (isWindow)
            {
                node.ForeColor = windowColor;
                nodeData.isWindow = true;
                allWindowsNodes.Add(node);

                if (handle != IntPtr.Zero) {
                    nodeData.handle = handle;
                    nodeData.isCurrentApp = (this.Handle == nodeData.handle);                    

                    if (nodeData.handle != null)
                    {
                        nodeData.title = TaskManager.getWindowTitle(handle);
                    }

                    nodeData.image = TaskManager.GetSmallWindowIcon(nodeData.handle);
                    nodeData.imageBase = ImageManager.ImageToString(nodeData.image);
                    this.imageList.Images.Add("image" + this.lastImageIndex, (Image)nodeData.image);                    
                    nodeData.imageIndex = this.lastImageIndex++;
                    node.ImageIndex = nodeData.imageIndex;
                    node.SelectedImageIndex = nodeData.imageIndex;
                }
           
            } else
            if (isInactiveWindow)
            {
                node.ForeColor = inactiveWindowColor;
                nodeData.isInactiveWindow = true;
                node.ForeColor = inactiveWindowColor;
                allInactiveWindowsNodes.Add(node);
            }

            allNodes.Add(node);

            if (parent != null)
            {
                NodeDataModel parentData = (NodeDataModel)parent.Tag;
                nodeData.parent = parentData.id;
                parent.Nodes.Add(node);
            }

            return node;
        }

        public void RemoveNode(TreeNode node)
        {
            Log.write("RemoveNode");
            List<TreeNode> subnodes = new List<TreeNode>();

            getNodes(subnodes, node);

            foreach (TreeNode n in subnodes)
            {
                if (n == rootNode || n == windowsRootNode)
                {
                    return;
                }
            }

            subnodes.Reverse();

            foreach (TreeNode n in subnodes)
            {
                allWindowsNodes.Remove(node);
                allInactiveWindowsNodes.Remove(node);
                allFolderNodes.Remove(node);
                allNoteNodes.Remove(node);
                allLinkNodes.Remove(node);
                allNodes.Remove(node);
                n.Remove();
            }

        }

        public void getNodes(List<TreeNode> nodes, TreeNode node, int level = 100)
        {
            if (level == 0) return;
            nodes.Add(node);
            foreach (TreeNode child in node.Nodes)
            {
                this.getNodes(nodes, child, level - 1);
            }
        }

        public void restoreNodes(List<TreeNode> list, TreeNode parent, int level = 100)
        {
            if (level == 0) return;
            NodeDataModel parentData = (NodeDataModel)parent.Tag;

            foreach (TreeNode element in list)
            {
                NodeDataModel nodeData = (NodeDataModel)element.Tag;

                if (parentData.id == nodeData.parent)
                {
                    parent.Nodes.Add(element);                    
                    this.restoreNodes(list, element, level - 1);
                }
            }
        }


        /* TREEVIEW EVENTS */

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Log.write("treeView_NodeMouseClick");

            if (e.Node != null) {
                treeView.SelectedNode = e.Node;
            }

            if (e.Node.Tag == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)e.Node.Tag;

            if (nodeData == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Left) {

                if (nodeData.isWindow && nodeData.handle != IntPtr.Zero && TaskManager.isLive(nodeData.handle)) {
                    TaskManager.setForegroundWindow(nodeData.handle);
                }

                if (nodeData.isInactiveWindow && nodeData.runCommand != "" && File.Exists(nodeData.runCommand))
                {
                    SystemManager.runApplication(nodeData.runCommand); 
                }
            }

        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Log.write("treeView_NodeMouseDoubleClick");

            if (e.Node != null)
            {
                treeView.SelectedNode = e.Node;
            }

            if (e.Node.Tag == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)e.Node.Tag;

            if (nodeData == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (nodeData.isLink && nodeData.runCommand != "")
                {
                    if (File.Exists(nodeData.runCommand))
                    {
                        SystemManager.runApplication(nodeData.runCommand);
                    }

                    if (Directory.Exists(nodeData.runCommand))
                    {
                        SystemManager.openDirectory(nodeData.runCommand);
                    }
                }
            }
        }

        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            Log.write("treeView mouse click");

            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);

            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = targetNode;
            }

        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            Log.write("treeView mouse up");
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = treeView.GetNodeAt(e.X, e.Y);

                if (treeView.SelectedNode != null)
                {
                    contextMenuStrip.Show(treeView, e.Location);
                }
            }
        }


        /* TREEVIEW EXPAND */

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)node.Tag;

            if (nodeData == null)
            {
                return;
            }

            nodeData.isExpanded = true;
        }

        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)node.Tag;

            if (nodeData == null)
            {
                return;
            }

            nodeData.isExpanded = false;
        }


        /* DRAG AND DROP EVENTS */

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            Log.write("ContainsNode");

            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            return ContainsNode(node1, node2.Parent);
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            Log.write("treeView_ItemDrag");
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            Log.write("treeView_DragEnter");
            e.Effect = e.AllowedEffect;
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            Log.write("treeView_DragOver");
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            treeView.SelectedNode = treeView.GetNodeAt(targetPoint);

            TreeNode targetNode = treeView.SelectedNode;

            if (targetNode == null) {
                return;
            }
            Rectangle targetNodeBounds = treeView.SelectedNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2 * blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2 * blockSize <= targetPoint.Y);

            Graphics g = treeView.CreateGraphics();
            Pen customPen = new Pen(Color.DimGray, 3) { DashStyle = DashStyle.Dash };
            Pen customPen2 = new Pen(SystemColors.Control, 3);

            g.DrawLine(customPen2, new Point(0, targetNode.Bounds.Top + 1), new Point(treeView.Width - 4, targetNode.Bounds.Top + 1));
            g.DrawLine(customPen2, new Point(0, targetNode.Bounds.Bottom - 1), new Point(treeView.Width - 4, targetNode.Bounds.Bottom - 1));

            if (addNodeUp)
            {
                g.DrawLine(customPen, new Point(0, targetNode.Bounds.Top+1), new Point(treeView.Width - 4, targetNode.Bounds.Top+1));
            }

            if (addNodeIn)
            {
                
            }

            if (addNodeDown)
            {
                g.DrawLine(customPen, new Point(0, targetNode.Bounds.Bottom-1), new Point(treeView.Width - 4, targetNode.Bounds.Bottom-1));
            }

            customPen.Dispose();
            g.Dispose();
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Log.write("treeView_DragDrop");
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            NodeDataModel targetNodeData = (NodeDataModel)targetNode.Tag;

            Rectangle targetNodeBounds = targetNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2 * blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2 * blockSize <= targetPoint.Y);

            if (e.Data.GetDataPresent(DataFormats.Text, false))
            {
                string text = (string)(e.Data.GetData(DataFormats.Text, false));
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] systemDrop = (string[])(e.Data.GetData(DataFormats.FileDrop, false));

                foreach (string path in systemDrop)
                {
                    if (!File.Exists(path) && !Directory.Exists(path)) {
                        continue;
                    }

                    string name = "Link";

                    if (Directory.Exists(path)) {
                        name = new DirectoryInfo(path).Name;
                    }

                    if (File.Exists(path))
                    {
                        name = new FileInfo(path).Name;
                    }


                    TreeNode linkNode = this.CreateNode(
                        IntPtr.Zero,
                        name,
                        path,
                        null,
                        false,
                        false,
                        false,
                        false,
                        true
                    );

                    NodeDataModel linkNodeData = (NodeDataModel)linkNode.Tag;

                    if (addNodeUp && !targetNodeData.isRoot)
                    {
                        int targetNodePosition = targetNode.Parent.Nodes.IndexOf(targetNode);
                        targetNode.Parent.Nodes.Insert(targetNodePosition, linkNode);
                        linkNodeData.parent = ((NodeDataModel)targetNode.Parent.Tag).id;
                    }

                    if (addNodeIn)
                    {
                        targetNode.Nodes.Add(linkNode);
                        targetNode.Expand();
                    }

                    if (addNodeDown && !targetNodeData.isRoot)
                    {
                        if (targetNode.Nodes.Count > 0 && targetNode.IsExpanded)
                        {
                            int targetNodePosition = 0;
                            targetNode.Nodes.Insert(targetNodePosition + 1, linkNode);
                            linkNodeData.parent = targetNodeData.id;
                        }
                        else {
                            int targetNodePosition = targetNode.Parent.Nodes.IndexOf(targetNode);
                            targetNode.Parent.Nodes.Insert(targetNodePosition + 1, linkNode);
                            linkNodeData.parent = ((NodeDataModel)targetNode.Parent.Tag).id;
                        }                        
                    }
                }

                return;
            }


            if (targetNode == null) {
                treeView.Invalidate();
                return;
            }

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (draggedNode == null) {
                treeView.Invalidate();
                return;
            }

            NodeDataModel draggedNodeData = (NodeDataModel)draggedNode.Tag;

            if (ContainsNode(draggedNode, targetNode)) {
                treeView.Invalidate();
                return;
            }

            if (e.Effect == DragDropEffects.Move)
            {
    
                if (addNodeUp && !targetNodeData.isRoot)
                {
                    int previousNodeIndex = targetNode.Parent.Nodes.IndexOf(targetNode) - 1;

                    // mov to nodebefore target node as last child
                    if (previousNodeIndex >=0 && targetNode.Parent.Nodes[previousNodeIndex].Nodes.Count > 0 && targetNode.Parent.Nodes[previousNodeIndex].IsExpanded)
                    {
                        TreeNode previousNode = targetNode.Parent.Nodes[previousNodeIndex];
                        draggedNode.Remove();
                        previousNode.Nodes.Add(draggedNode);
                        draggedNodeData.parent = ((NodeDataModel)targetNode.Parent.Nodes[previousNodeIndex].Tag).id;
                        
                    }
                    else
                    {
                        // move before target node
                        if (!draggedNode.Equals(targetNode)) {
                            TreeNode targetParentNode = targetNode.Parent;
                            draggedNode.Remove();
                            int targetNodePosition = targetParentNode.Nodes.IndexOf(targetNode);
                            targetParentNode.Nodes.Insert(targetNodePosition, draggedNode);
                            draggedNodeData.parent = ((NodeDataModel)targetParentNode.Tag).id;
                        }
                    }
                }

                // move to target node
                if (addNodeIn && (!draggedNode.Equals(targetNode)))
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();

                    draggedNodeData.parent = targetNodeData.id;
                }

                if (addNodeDown && !targetNodeData.isRoot)
                {
                    // move to target child node as firs child
                    if (targetNode.Nodes.Count > 0 && targetNode.IsExpanded)
                    {
                        draggedNode.Remove();
                        int targetNodePosition = 0;
                        targetNode.Nodes.Insert(targetNodePosition, draggedNode);
                        draggedNodeData.parent = targetNodeData.id;
                        
                    }
                    else
                    {
                        // move after target node
                        if (!draggedNode.Equals(targetNode))
                        {
                            TreeNode targetParentNode = targetNode.Parent;
                            draggedNode.Remove();
                            int targetNodePosition = targetParentNode.Nodes.IndexOf(targetNode);
                            targetNode.Parent.Nodes.Insert(targetNodePosition + 1, draggedNode);
                            draggedNodeData.parent = ((NodeDataModel)targetParentNode.Tag).id;
                        }
                    }
                }
            }

            treeView.Invalidate();

        }

        /* CONTEXT MENU EVENTS */

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            TreeNode node = treeView.SelectedNode;

            if (node == null) {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)node.Tag;

            pinToolStripMenuItem.Checked = nodeData.isPinned;

            if (pinToolStripMenuItem.Checked)
            {
                pinToolStripMenuItem.Text = "Unpin";
            }
            else {
                pinToolStripMenuItem.Text = "Pin";
            }


        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("renameToolStripMenuItem_Click");

            if (treeView.SelectedNode == null)
            {
                return;
            }


            TreeNode parentNode = treeView.SelectedNode;
                        
            this.CreateNode(
                IntPtr.Zero,
                "Folder",
                null,
                parentNode,
                true,
                false,
                false,
                false,
                false
            );

            treeView.SelectedNode.Expand();
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("noteToolStripMenuItem_Click");

            if (treeView.SelectedNode == null)
            {
                return;
            }


            TreeNode parentNode = treeView.SelectedNode;

            this.CreateNode(
                IntPtr.Zero,
                "Note",
                null,
                parentNode,
                false,
                false,
                false,
                true,
                false
            );

            treeView.SelectedNode.Expand();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("closeToolStripMenuItem_Click");
            if (treeView.SelectedNode == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)treeView.SelectedNode.Tag;

            if (nodeData.handle != IntPtr.Zero) {
                TaskManager.CloseWindow(nodeData.handle);
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("renameToolStripMenuItem_Click");

            if (treeView.SelectedNode == null)
            {
                return;
            }

            treeView.LabelEdit = true;
            treeView.SelectedNode.BeginEdit();
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            Log.write("treeView_AfterLabelEdit");

            if (e.Label == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)e.Node.Tag;

            if (nodeData == null)
            {
                return;
            }


            if (nodeData.isWindow) {
                if (e.Label != "" && e.Label != nodeData.windowTitle) {
                    nodeData.title = e.Label;
                    nodeData.isRenamed = true;
                } else {
                    nodeData.title = nodeData.windowTitle;
                    nodeData.isRenamed = false;
                }
            } else {
                nodeData.title = e.Label;
                nodeData.isRenamed = false;
            }

            e.Node.Text = nodeData.title;
            e.Node.EndEdit(true);
            treeView.LabelEdit = false;

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("deleteToolStripMenuItem_Click");

            
            if (treeView.SelectedNode == null)
            {
                return;
            }

            RemoveNode(treeView.SelectedNode);

        }

        private void pinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("pinToolStripMenuItem_Click");

            if(treeView.SelectedNode == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)treeView.SelectedNode.Tag;
            if (nodeData.isWindow || nodeData.isInactiveWindow) {
                nodeData.isPinned = !nodeData.isPinned;
            }
        }
        
        /* MENU EVENTS */

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("lockToolStripMenuItem_Click");
            SystemManager.Lock();
        }

        private void sleepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("sleepToolStripMenuItem_Click");
            Application.SetSuspendState(PowerState.Suspend, true, true);
        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("signOutToolStripMenuItem_Click");
            SystemManager.SignOut();
        }

        private void hibernateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("hibernateToolStripMenuItem_Click");
            Application.SetSuspendState(PowerState.Hibernate, true, true);
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("restartToolStripMenuItem_Click");
            SystemManager.Restart();
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("shutdownToolStripMenuItem_Click");
            SystemManager.ShutDown();
        }

        public void setTopMost(bool chcecked)
        {
            Log.write("setTopMost");
            this.TopMost = chcecked;
            alwaysOnTopToolStripMenuItem.Checked = chcecked;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("alwaysOnTopToolStripMenuItem_Click");
            this.TopMost = !this.TopMost;
            alwaysOnTopToolStripMenuItem.Checked = this.TopMost;
        }

        private void autorunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("autorunToolStripMenuItem_Click");

            if (SystemManager.isAutorunSet())
            {
                SystemManager.autorunOff();
                autorunToolStripMenuItem.Checked = false;
            }
            else {
                SystemManager.autorunOn();
                autorunToolStripMenuItem.Checked = true;
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("fontToolStripMenuItem_Click");
            fontDialog.Font = treeView.Font;
            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                treeView.Font = fontDialog.Font;
            }
        }

        private void rememberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("rememberToolStripMenuItem_Click");
            this.options.rememberState = !this.options.rememberState;
            rememberToolStripMenuItem.Checked = this.options.rememberState;
        }

        private void showInTaskbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("showInTaskbarToolStripMenuItem_Click");
            this.options.ShowInTaskbar = !this.options.ShowInTaskbar;
            showInTaskbarToolStripMenuItem.Checked = this.options.ShowInTaskbar;

            if (!this.options.ShowInTaskbar)
            {

                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else
            {
                this.ShowInTaskbar = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }

            // expand nodes
            foreach (TreeNode node in allNodes)
            {
                NodeDataModel nodeData = (NodeDataModel)node.Tag;
                if (nodeData.isExpanded)
                {
                    node.Expand();
                }
                else
                {
                    node.Collapse();
                }
            }
        }

        private void showDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("showDesktopToolStripMenuItem_Click");
            TaskManager.ShowDesktop();
        }

        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemManager.soundMute(this.Handle);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SystemManager.soundLevel(25);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SystemManager.soundLevel(50);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            SystemManager.soundLevel(75);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            SystemManager.soundLevel(100);
        }

        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {

        }


        private void treeView_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            SolidBrush myBrush = new SolidBrush(e.Node == treeView.SelectedNode ? Color.FromArgb(204, 204, 255):SystemColors.Control);
            e.Graphics.FillRectangle(myBrush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, e.Node.ForeColor, TextFormatFlags.GlyphOverhangPadding);
        }
    }
}
