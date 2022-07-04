namespace TaskList
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelVersionNumber = new System.Windows.Forms.Label();
            this.labelAppName = new System.Windows.Forms.Label();
            this.labelEnv = new System.Windows.Forms.Label();
            this.labelBranch = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(28, 76);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(91, 25);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Version:";
            // 
            // labelVersionNumber
            // 
            this.labelVersionNumber.AutoSize = true;
            this.labelVersionNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersionNumber.Location = new System.Drawing.Point(125, 76);
            this.labelVersionNumber.Name = "labelVersionNumber";
            this.labelVersionNumber.Size = new System.Drawing.Size(60, 25);
            this.labelVersionNumber.TabIndex = 0;
            this.labelVersionNumber.Text = "1.0.0";
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppName.Location = new System.Drawing.Point(12, 9);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(202, 55);
            this.labelAppName.TabIndex = 1;
            this.labelAppName.Text = "TaskList";
            // 
            // labelEnv
            // 
            this.labelEnv.AutoSize = true;
            this.labelEnv.Location = new System.Drawing.Point(191, 85);
            this.labelEnv.Name = "labelEnv";
            this.labelEnv.Size = new System.Drawing.Size(37, 13);
            this.labelEnv.TabIndex = 2;
            this.labelEnv.Text = "debug";
            // 
            // labelBranch
            // 
            this.labelBranch.AutoSize = true;
            this.labelBranch.Location = new System.Drawing.Point(246, 85);
            this.labelBranch.Name = "labelBranch";
            this.labelBranch.Size = new System.Drawing.Size(40, 13);
            this.labelBranch.TabIndex = 3;
            this.labelBranch.Text = "branch";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 175);
            this.Controls.Add(this.labelBranch);
            this.Controls.Add(this.labelEnv);
            this.Controls.Add(this.labelAppName);
            this.Controls.Add(this.labelVersionNumber);
            this.Controls.Add(this.labelVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About TaskList";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelVersionNumber;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.Label labelEnv;
        private System.Windows.Forms.Label labelBranch;
    }
}