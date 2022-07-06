using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskList
{
    public partial class ExcludeRuleEditForm : Form
    {
        public bool delete = false; 
        public bool canceled = false; 
        public bool ok = false;

        public ExcludeRule excludeRule = new ExcludeRule();

        public ExcludeRuleEditForm()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.canceled = true;
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.ok = true;
            this.excludeRule.name = this.textBoxName.Text;
            this.excludeRule.excludeRule = this.textBoxRule.Text;
            this.Close();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            this.delete = true;
            this.Close();
        }

        public void hideDeleteButton() { 
            this.buttonDelete.Hide();
        }

        private void ExcludeRuleEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.canceled = true;
        }
    }
}
