using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
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
        //TreeNode processiesNode = null;
        TreeNode windowsNode = null;

        List<TreeNode> allNodes = new List<TreeNode>();
        //List<TreeNode> allProcessiesNodes = new List<TreeNode>();
        List<TreeNode> allWindowsNodes = new List<TreeNode>();
        List<TreeNode> allFolderNodes = new List<TreeNode>();

        ImageList imageList = new ImageList();
        int lastNodeIndex = 0;
        int lastImageIndex = 0;
        int defaultIconIndex = 0;
        int folderIconIndex = 0;

        /* FORM EVENTS */

        public TaskListForm()
        {
            Log.write("Constructor");
            InitializeComponent();
        }

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
            options.Add(new XElement("Maximised", this.options.Maximised?"1":"0"));
            options.Add(new XElement("Maximised", this.options.Minimised ? "1" : "0"));
            options.Add(new XElement("FirstRun", this.options.firstRun ? "1" : "0"));
            options.Add(new XElement("MostTop", this.options.mostTop ? "1" : "0"));
            options.Add(new XElement("Font", this.options.font));
            options.Add(new XElement("RememberState", this.options.rememberState ? "1" : "0"));
            options.Add(new XElement("ShowInTaskbar", this.options.ShowInTaskbar ? "1" : "0"));

            XElement nodes = new XElement("Nodes");
            options.Add(nodes);

            List<TreeNode> allNodes = new List<TreeNode>();
                
            this.getNodes(allNodes, rootNode);

            foreach (TreeNode node in allNodes) {
                XElement nodeElement = new XElement("Node");

                NodeDataModel nodeData = (NodeDataModel)node.Tag;

                nodeElement.Add(new XElement("id", nodeData.id));
                nodeElement.Add(new XElement("name", node.Text));
                nodeElement.Add(new XElement("parent", nodeData.parent));
                nodeElement.Add(new XElement("isRoot", nodeData.isRoot ? "1" : "0"));
                nodeElement.Add(new XElement("isWindow", nodeData.isWindow ? "1" : "0"));
                nodeElement.Add(new XElement("isFolder", nodeData.isFolder ? "1" : "0"));
                nodeElement.Add(new XElement("isDeletable", nodeData.isDeletable ? "1" : "0"));
                nodeElement.Add(new XElement("isMoovable", nodeData.isMoovable ? "1" : "0"));
                nodeElement.Add(new XElement("isHidden", nodeData.isHidden ? "1" : "0"));
                nodeElement.Add(new XElement("isRenamed", nodeData.isRenamed ? "1" : "0"));
                nodeElement.Add(new XElement("isCurrentApp", nodeData.isCurrentApp ? "1" : "0"));   

                nodes.Add(nodeElement);
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(settingsFilePath);
            file.Write(options);
            file.Close();
        }

        public void getNodes(List<TreeNode> nodes, TreeNode node) {
            nodes.Add(node);
            foreach (TreeNode child in node.Nodes) {
                this.getNodes(nodes, child);
            }
        }

        public void restoreNodes(List<TreeNode> list, TreeNode parent)
        {

            NodeDataModel parentData = (NodeDataModel)parent.Tag;

            foreach (TreeNode element in list)
            {
                NodeDataModel nodeData = (NodeDataModel)element.Tag;

                if (parentData.id == nodeData.parent) {
                    parent.Nodes.Add(element);
                    this.restoreNodes(list, element);
                }
            }
        }

        public decimal StringToDecimal(string text)
        {
            return decimal.Parse(
                text,
                NumberStyles.AllowParentheses |
                NumberStyles.AllowLeadingWhite |
                NumberStyles.AllowTrailingWhite |
                NumberStyles.AllowThousands |
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowLeadingSign,
                NumberFormatInfo.InvariantInfo
            );
        }

        public void restoreFormSettings()
        {
            Log.write("FormManager restoreFormPosition");

#if DEBUG
            string settingsFilePath = "settings.xml";
#else
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TaskList\\";
            string settingsFilePath = roamingPath + "settings.xml";
#endif

            if (!File.Exists(settingsFilePath)) {
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

            TreeNode rootNode = null;
            List<TreeNode> allNodes = new List<TreeNode>();

            try
            {
                using (XmlReader xr = XmlReader.Create(new StringReader(xml), xws))
                {

                    XElement root = XElement.Load(xr);
                    
                    foreach (XElement option in root.Elements())
                    {
                        if (option.Name.ToString() == "LocationX")
                        {
                            this.options.Location.X = (int)StringToDecimal(option.Value);
                        }

                        if (option.Name.ToString() == "LocationY")
                        {
                            this.options.Location.Y = (int)StringToDecimal(option.Value);
                        }

                        if (option.Name.ToString() == "SizeWidth")
                        {
                            this.options.Size.Width = (int)StringToDecimal(option.Value);
                        }

                        if (option.Name.ToString() == "SizeHeight")
                        {
                            this.options.Size.Height = (int)StringToDecimal(option.Value);
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
                                        }

                                        if (attribute.Name.ToString() == "name")
                                        {
                                            newNode.Text = attribute.Value;
                                        }

                                        if (attribute.Name.ToString() == "parent")
                                        {
                                            newNodeData.parent = Int32.Parse(attribute.Value);
                                        }

                                        if (attribute.Name.ToString() == "isRoot")
                                        {
                                            newNodeData.isRoot = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isWindow")
                                        {
                                            newNodeData.isWindow = attribute.Value == "1";
                                        }

                                        if (attribute.Name.ToString() == "isFolder")
                                        {
                                            newNodeData.isFolder = attribute.Value == "1";
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
                                    }

                                    if (newNodeData.parent == 0) {

                                        rootNode = newNode;
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

            if (rootNode != null)
            {
                this.restoreNodes(allNodes, rootNode);
                treeView.Nodes.Add(rootNode);
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

            if (this.options.font != null)
            {
                this.treeView.Font = TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(this.options.font) as Font;
            }
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            Log.write("Load");

#if DEBUG
            this.Text += " - DEBUG";
#endif

            this.restoreFormSettings();

            treeView.BeginUpdate();

            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            treeView.ImageList = this.imageList;

            Bitmap defaultIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.TaskList.ico"));
            this.defaultIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.defaultIconIndex, (Image)defaultIcon);

            Bitmap folderIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.folder.ico"));
            this.folderIconIndex = this.lastImageIndex++;
            this.imageList.Images.Add("image" + this.folderIconIndex, (Image)folderIcon);

            var rootNodeData = new NodeDataModel();
            rootNodeData.id = ++this.lastNodeIndex;
            rootNodeData.parent = 0;
            rootNodeData.isRoot = true;
            rootNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.rootNode = treeView.Nodes.Add("Tasks", "Tasks", this.defaultIconIndex, this.defaultIconIndex);
            this.rootNode.Tag = rootNodeData;

            /*var processiesNodeData = new NodeDataModel();
            processiesNodeData.id = ++this.lastNodeIndex;
            rootNodeData.parent = rootNodeData.id;
            processiesNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.processiesNode = rootNode.Nodes.Add("Processies", "Processies", this.defaultIconIndex, this.defaultIconIndex);
            this.processiesNode.Tag = processiesNodeData;*/

            var windowsNodeData = new NodeDataModel();
            windowsNodeData.id = ++this.lastNodeIndex;
            windowsNodeData.parent = rootNodeData.id;
            windowsNodeData.isMoovable = false;
            this.windowsNode = rootNode.Nodes.Add("Windows", "Windows", this.defaultIconIndex, this.defaultIconIndex);
            rootNodeData.image = defaultIcon;
            this.windowsNode.Tag = windowsNodeData;

            this.updatTree();


            rootNode.Expand();

            windowsNode.Expand();
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

        public object Os { get; private set; }

        /* UPDATE TREEVIEW */

        private void update_Tick(object sender, EventArgs e)
        {
            Log.write("Update tick");
            this.updatTree();
        }

        private void updatTree()
        {
            Log.write("updatTree");

            IntPtr currentAppHandle = this.Handle;

            List<TreeNode> toRemoveNodes = new List<TreeNode>();

            


            // windows add 
            IDictionary<IntPtr, string> windowsList = TaskManager.GetOpenWindows();

            NodeDataModel windowsNodeData = (NodeDataModel)windowsNode.Tag;

            foreach (KeyValuePair<IntPtr, string> window in windowsList)
            {

                if (currentAppHandle == window.Key) {
                    continue;
                }

                bool exists = false;
                foreach (TreeNode oldNode in this.allWindowsNodes)
                {
                    var oldNodeData = (NodeDataModel)oldNode.Tag;

                    if (oldNodeData.handle == window.Key)
                    {

                        if (!oldNodeData.isRenamed && window.Value != oldNode.Text)
                        {
                            oldNode.Text = window.Value;
                        }

                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                var nodeData = new NodeDataModel();
                nodeData.id = ++this.lastNodeIndex;
                nodeData.parent = windowsNodeData.id;
                nodeData.isWindow = true;
                nodeData.process = null;
                nodeData.handle = window.Key;
                nodeData.isCurrentApp = currentAppHandle == nodeData.handle;
                nodeData.image = TaskManager.GetSmallWindowIcon(nodeData.handle);

                this.imageList.Images.Add("image" + this.lastImageIndex, (Image)nodeData.image);
                var node = windowsNode.Nodes.Add(window.Value, window.Value, this.lastImageIndex, this.lastImageIndex++);
                node.Tag = nodeData;

                this.allNodes.Add(node);
                this.allWindowsNodes.Add(node);

            }

            foreach (TreeNode oldNode in this.allWindowsNodes)
            {

                NodeDataModel nodeData = (NodeDataModel)oldNode.Tag;
                bool exists = false;
                foreach (KeyValuePair<IntPtr, string> window in windowsList)
                {
                    if (nodeData.handle == window.Key)
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

            foreach (TreeNode oldNode in toRemoveNodes)
            {
                this.allWindowsNodes.Remove(oldNode);
                this.allNodes.Remove(oldNode);

                treeView.Nodes.Remove(oldNode);
            }


            //treeView.EndUpdate();
        }

        /* TREEVIEW EVENTS */

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Log.write("node click");

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

            if (nodeData.handle == IntPtr.Zero)
            {
                return;
            }

            if (!TaskManager.isLive(nodeData.handle))
            {
                return;
            }

            if (e.Button == MouseButtons.Left) {
                TaskManager.setForegroundWindow(nodeData.handle);
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

            if (targetNode == null) {
                return;
            }

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            NodeDataModel draggedNodeData = (NodeDataModel)draggedNode.Tag;

            if (draggedNode.Equals(targetNode) || ContainsNode(draggedNode, targetNode))
            {
                return;
            }

            Rectangle targetNodeBounds = targetNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2*blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2*blockSize <= targetPoint.Y);


            if (e.Effect == DragDropEffects.Move)
            {
    
                if (addNodeUp && !targetNodeData.isRoot)
                {
                    draggedNode.Remove();
                    int targetNodePosition = targetNode.Parent.Nodes.IndexOf(targetNode);
                    targetNode.Parent.Nodes.Insert(targetNodePosition, draggedNode);
                }

                if (addNodeIn)
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();
                }

                if (addNodeDown && !targetNodeData.isRoot)
                {
                    draggedNode.Remove();
                    int targetNodePosition = targetNode.Parent.Nodes.IndexOf(targetNode);
                    targetNode.Parent.Nodes.Insert(targetNodePosition + 1, draggedNode);
                }
            }

            treeView.Invalidate();

        }

        /* CONTEXT MENU EVENTS */

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("renameToolStripMenuItem_Click");

            if (treeView.SelectedNode == null)
            {
                return;
            }


            TreeNode parentNode = treeView.SelectedNode;

            NodeDataModel parentNodeData = (NodeDataModel)parentNode.Tag;

            TreeNode node = parentNode.Nodes.Add("Folder", "Folder", this.folderIconIndex, this.folderIconIndex);

            NodeDataModel nodeData = new NodeDataModel();
            nodeData.id = ++this.lastNodeIndex;
            nodeData.parent = parentNodeData.id;
            nodeData.isFolder = true;
            node.Tag = nodeData;
            
            this.allNodes.Add(node);
            this.allFolderNodes.Add(node);
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

            nodeData.isRenamed = true;
            e.Node.EndEdit(true);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("deleteToolStripMenuItem_Click");

            if (treeView.SelectedNode == null)
            {
                return;
            }

            NodeDataModel nodeData = (NodeDataModel)treeView.SelectedNode.Tag;

            if (nodeData.isFolder && treeView.SelectedNode.Nodes.Count == 0) {
                this.allNodes.Remove(treeView.SelectedNode);
                this.allFolderNodes.Remove(treeView.SelectedNode);
                treeView.SelectedNode.Parent.Nodes.Remove(treeView.SelectedNode);
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
        }

        private void showDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("showDesktopToolStripMenuItem_Click");
            TaskManager.ShowDesktop();
        }


    }
}
