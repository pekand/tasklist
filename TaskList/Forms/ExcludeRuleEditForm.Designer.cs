namespace TaskList
{
    partial class ExcludeRuleEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExcludeRuleEditForm));
            this.labelName = new System.Windows.Forms.Label();
            this.labelRule = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxRule = new System.Windows.Forms.TextBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(35, 46);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(67, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name of rule";
            // 
            // labelRule
            // 
            this.labelRule.AutoSize = true;
            this.labelRule.Location = new System.Drawing.Point(34, 98);
            this.labelRule.Name = "labelRule";
            this.labelRule.Size = new System.Drawing.Size(58, 13);
            this.labelRule.TabIndex = 1;
            this.labelRule.Text = "Regex rule";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(118, 39);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(324, 20);
            this.textBoxName.TabIndex = 2;
            // 
            // textBoxRule
            // 
            this.textBoxRule.Location = new System.Drawing.Point(118, 95);
            this.textBoxRule.Name = "textBoxRule";
            this.textBoxRule.Size = new System.Drawing.Size(324, 20);
            this.textBoxRule.TabIndex = 3;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(367, 148);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(37, 145);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 26);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(34, 191);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(207, 13);
            this.labelInfo.TabIndex = 6;
            this.labelInfo.Text = "Task matching regex rule will be excluded.";
            // 
            // labelInfo2
            // 
            this.labelInfo2.AutoSize = true;
            this.labelInfo2.Location = new System.Drawing.Point(247, 191);
            this.labelInfo2.Name = "labelInfo2";
            this.labelInfo2.Size = new System.Drawing.Size(229, 13);
            this.labelInfo2.TabIndex = 7;
            this.labelInfo2.Text = "Name is only for easy rule identification in menu";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(118, 148);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 8;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // ExcludeRuleEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 225);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.textBoxRule);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.labelRule);
            this.Controls.Add(this.labelName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExcludeRuleEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Task Exclude Rule";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExcludeRuleEditForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelRule;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxRule;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelInfo2;
        private System.Windows.Forms.Button buttonDelete;
    }
}