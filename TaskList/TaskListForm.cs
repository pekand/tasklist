using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TaskList
{
    public partial class TaskListForm : Form
    {

        TreeNode rootNode = null;
        TreeNode processiesNode = null;
        TreeNode windowsNode = null;

        List<TreeNode> allNodes = new List<TreeNode>();
        List<TreeNode> allProcessiesNodes = new List<TreeNode>();
        List<TreeNode> allWindowsNodes = new List<TreeNode>();

        ImageList imageList = new ImageList();
        int imageIndex = 0;

        /* FORM EVENTS */

        public TaskListForm()
        {
            Log.write("Constructor");
            InitializeComponent();
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            Log.write("Load");
            treeView.BeginUpdate();

            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            treeView.ImageList = this.imageList;

            Assembly assembly = Assembly.GetExecutingAssembly();
            //string[] names = assembly.GetManifestResourceNames();

            Bitmap defaultIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.TaskList.ico"));

            this.imageList.Images.Add("image" + this.imageIndex, (Image)defaultIcon);

            var rootNodeData = new NodeDataModel();
            rootNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.rootNode = treeView.Nodes.Add("Tasks", "Tasks", this.imageIndex, this.imageIndex++);
            this.rootNode.Tag = rootNodeData;

            var processiesNodeData = new NodeDataModel();
            processiesNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.processiesNode = rootNode.Nodes.Add("Processies", "Processies", 0, 0);
            this.processiesNode.Tag = processiesNodeData;

            var windowsNodeData = new NodeDataModel();
            windowsNodeData.isMoovable = false;
            this.windowsNode = rootNode.Nodes.Add("Windows", "Windows", 0, 0);
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
            FormManager.restoreFormPosition(this);
        }

        private void TaskList_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.write("Closing");
            FormManager.saveFormPosition(this);
        }

        /* UPDATE TREEVIEW */

        private void update_Tick(object sender, EventArgs e)
        {
            Log.write("Update");
            this.updatTree();
        }

        private void updatTree()
        {
            Log.write("updatTree");
            //treeView.BeginUpdate();

            //process add 

            Process[] processies = TaskManager.getProcessies();

            List<TreeNode> toRemoveNodes = new List<TreeNode>();

            foreach (Process process in processies)
            {
                bool exists = false;
                foreach (TreeNode oldNode in this.allProcessiesNodes)
                {
                    var oldNodeData = (NodeDataModel)oldNode.Tag;

                    if (oldNodeData.process.Id == process.Id)
                    {

                        if (!oldNodeData.isRenamed && process.ProcessName != oldNode.Text) {
                            oldNode.Text = process.ProcessName;
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
                nodeData.isProcess = true;
                nodeData.process = process;

                try
                {
                    /* todo: it is slow  skip for now */
                    /*nodeData.image = TaskManager.GetSmallWindowIcon(process.Handle);
                    this.imageList.Images.Add("image" + this.imageIndex, (Image)nodeData.image);*/
                }
                catch (System.ComponentModel.Win32Exception e) {
                    Log.write("Exception: " + e.Message);
                    nodeData.image = null;
                }
                catch (System.InvalidOperationException e)
                {
                    Log.write("Exception: " + e.Message);
                    nodeData.image = null;
                }

                TreeNode node = null;
                if (nodeData.image == null) {
                    node = processiesNode.Nodes.Add(process.ProcessName);
                }
                else {
                    node = processiesNode.Nodes.Add(process.ProcessName, process.ProcessName, this.imageIndex, this.imageIndex++);
                }

                node.Tag = nodeData;

                this.allNodes.Add(node);
                this.allProcessiesNodes.Add(node);
            }


            foreach (TreeNode oldNode in this.allProcessiesNodes)
            {

                NodeDataModel nodeData = (NodeDataModel)oldNode.Tag;
                bool exists = false;
                foreach (Process process in processies)
                {
                    if (nodeData.process.Id == process.Id)
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


            // windows add 
            IDictionary<IntPtr, string> windowsList = TaskManager.GetOpenWindows();

            foreach (KeyValuePair<IntPtr, string> window in windowsList)
            {
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
                nodeData.isWindow = true;
                nodeData.process = null;
                nodeData.handle = window.Key;
                nodeData.image = TaskManager.GetSmallWindowIcon(nodeData.handle);

                this.imageList.Images.Add("image" + this.imageIndex, (Image)nodeData.image);
                var node = windowsNode.Nodes.Add(window.Value, window.Value, this.imageIndex, this.imageIndex++);
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
                this.allProcessiesNodes.Remove(oldNode);
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
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Log.write("treeView_DragDrop");
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {

                if (e.Effect == DragDropEffects.Move)
                {
                    
                    int targetNodePosition =  targetNode.Parent.Nodes.IndexOf(targetNode);

                    if (targetNode.Parent.Nodes.Count-1 == targetNodePosition) {
                        targetNodePosition++;
                    }

                    draggedNode.Remove();

                    targetNode.Parent.Nodes.Insert(targetNodePosition, draggedNode);

                }

                else if (e.Effect == DragDropEffects.Copy)
                {
                    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                targetNode.Expand();
            }
        }

        /* CONTEXT MENU EVENTS */
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

        private void showDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.write("showDesktopToolStripMenuItem_Click");
            TaskManager.ShowDesktop();
        }

    }
}
