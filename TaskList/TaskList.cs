using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shell32;

namespace TaskList
{
    public partial class TaskList : Form
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

        public TaskList()
        {
            InitializeComponent();
        }

        private void TaskList_Load(object sender, EventArgs e)
        {
            treeView.BeginUpdate();

            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            treeView.ImageList = this.imageList;

            Assembly assembly = Assembly.GetExecutingAssembly();
            //string[] names = assembly.GetManifestResourceNames();

            Bitmap defaultIcon = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("TaskList.Resources.TaskList.ico"));

            this.imageList.Images.Add("image" + this.imageIndex, (Image)defaultIcon);

            var rootNodeData = new NodeData();
            rootNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.rootNode = treeView.Nodes.Add("Tasks", "Tasks", this.imageIndex, this.imageIndex++);
            this.rootNode.Tag = rootNodeData;

            var processiesNodeData = new NodeData();
            processiesNodeData.isMoovable = false;
            rootNodeData.image = defaultIcon;
            this.processiesNode = rootNode.Nodes.Add("Processies", "Processies", 0, 0);
            this.processiesNode.Tag = processiesNodeData;

            var windowsNodeData = new NodeData();
            windowsNodeData.isMoovable = false;
            this.windowsNode = rootNode.Nodes.Add("Windows", "Windows", 0, 0);
            rootNodeData.image = defaultIcon;
            this.windowsNode.Tag = windowsNodeData;

            this.updatTree();

            rootNode.Expand();

            windowsNode.Expand();
            treeView.EndUpdate();


            this.updateTimer.Enabled = true;
        }

        private void TaskList_Shown(object sender, EventArgs e)
        {
            FormManager.restoreFormPosition(this);
        }

        private void TaskList_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormManager.saveFormPosition(this);
        }

        /* UPDATE TREEVIEW */

        private void update_Tick(object sender, EventArgs e)
        {
            this.updatTree();
        }

        private void updatTree()
        {
            //treeView.BeginUpdate();

            //process add 

            Process[] processies = FormManager.getProcessies();

            List<TreeNode> toRemoveNodes = new List<TreeNode>();

            foreach (Process process in processies)
            {
                bool exists = false;
                foreach (TreeNode oldNode in this.allProcessiesNodes)
                {

                    if (((NodeData)oldNode.Tag).process.Id == process.Id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                var nodeData = new NodeData();
                nodeData.isProcess = true;
                nodeData.process = process;

                try
                {
                    /* todo: it is slow  skip for now */
                    /*nodeData.image = TaskManager.GetSmallWindowIcon(process.Handle);
                    this.imageList.Images.Add("image" + this.imageIndex, (Image)nodeData.image);*/
                }
                catch (System.ComponentModel.Win32Exception e) {
                    nodeData.image = null;
                }
                catch (System.InvalidOperationException e)
                {
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

                NodeData nodeData = (NodeData)oldNode.Tag;
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

                    if (((NodeData)oldNode.Tag).handle == window.Key)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                var nodeData = new NodeData();
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

                NodeData nodeData = (NodeData)oldNode.Tag;
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

        private void treeView_Click(object sender, EventArgs e)
        {

        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {

        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                return;
            }

            NodeData nodeData = (NodeData)e.Node.Tag;

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

            TaskManager.setForegroundWindow(nodeData.handle);
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null) {
                return;
            }

            NodeData nodeData  = (NodeData)e.Node.Tag;

            
            if (nodeData == null) {
                return;
            }

            if (nodeData.handle ==  IntPtr.Zero) {
                return;
            }

            if (!TaskManager.isLive(nodeData.handle)) {
                return;
            }

            TaskManager.setForegroundWindow(nodeData.handle);

        }


        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {

            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);

            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = targetNode;
            }

        }

        /* DRAG AND DROP EVENTS */

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {

            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            return ContainsNode(node1, node2.Parent);
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            treeView.SelectedNode = treeView.GetNodeAt(targetPoint);
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
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

        /* STRIP MENU EVENTS */
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemManager.Lock();
        }

        private void sleepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.SetSuspendState(PowerState.Suspend, true, true);
        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemManager.SignOut();
        }

        private void hibernateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.SetSuspendState(PowerState.Hibernate, true, true);
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemManager.Restart();
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemManager.ShutDown();
        }

        public void setTopMost(bool chcecked)
        {
            this.TopMost = chcecked;
            alwaysOnTopToolStripMenuItem.Checked = chcecked;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            alwaysOnTopToolStripMenuItem.Checked = this.TopMost;
        }

        private void showDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shell32.Shell shell = new Shell32.Shell();
            shell.ToggleDesktop();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
