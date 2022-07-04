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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TaskList
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private string getVersion() {
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
                            return  version;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "unknown";
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("info.txt"));

            string result = "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            string[] lines = Regex.Split(result, "\n");

            labelVersionNumber.Text = lines[0].Trim();
            labelBranch.Text = lines[1].Trim();
            labelEnv.Text = lines[2].Trim();
        }
    }
}
