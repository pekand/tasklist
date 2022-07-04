using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TaskList
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void getVersion() {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\pekand\\TaskList"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("version");
                        if (o != null)
                        {
                            string version = o as String;
                            labelVersionNumber.Text = version;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                labelVersionNumber.Text = "unknown";
            }
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            this.getVersion();
        }
    }
}
