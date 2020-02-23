namespace WinformsToastHost
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ShowToast = new System.Windows.Forms.Button();
            this.textblockPackageId = new System.Windows.Forms.TextBox();
            this.textblockPackagePath = new System.Windows.Forms.TextBox();
            this.LoadAssembly = new System.Windows.Forms.Button();
            this.Message = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 93);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Package Identity:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(19, 188);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(225, 37);
            this.label2.TabIndex = 1;
            this.label2.Text = "Package Path:";
            // 
            // ShowToast
            // 
            this.ShowToast.Location = new System.Drawing.Point(25, 295);
            this.ShowToast.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.ShowToast.Name = "ShowToast";
            this.ShowToast.Size = new System.Drawing.Size(560, 161);
            this.ShowToast.TabIndex = 2;
            this.ShowToast.Text = "Show Toast";
            this.ShowToast.UseVisualStyleBackColor = true;
            this.ShowToast.Click += new System.EventHandler(this.button1_Click);
            // 
            // textblockPackageId
            // 
            this.textblockPackageId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textblockPackageId.Location = new System.Drawing.Point(279, 84);
            this.textblockPackageId.Name = "textblockPackageId";
            this.textblockPackageId.Size = new System.Drawing.Size(1040, 44);
            this.textblockPackageId.TabIndex = 3;
            // 
            // textblockPackagePath
            // 
            this.textblockPackagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textblockPackagePath.Location = new System.Drawing.Point(279, 185);
            this.textblockPackagePath.Name = "textblockPackagePath";
            this.textblockPackagePath.Size = new System.Drawing.Size(1040, 44);
            this.textblockPackagePath.TabIndex = 4;
            // 
            // LoadAssembly
            // 
            this.LoadAssembly.Location = new System.Drawing.Point(25, 484);
            this.LoadAssembly.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.LoadAssembly.Name = "LoadAssembly";
            this.LoadAssembly.Size = new System.Drawing.Size(560, 161);
            this.LoadAssembly.TabIndex = 5;
            this.LoadAssembly.Text = "Load Assembly";
            this.LoadAssembly.UseVisualStyleBackColor = true;
            this.LoadAssembly.Click += new System.EventHandler(this.button2_Click);
            // 
            // Message
            // 
            this.Message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Message.Location = new System.Drawing.Point(609, 543);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(710, 44);
            this.Message.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1458, 815);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.LoadAssembly);
            this.Controls.Add(this.textblockPackagePath);
            this.Controls.Add(this.textblockPackageId);
            this.Controls.Add(this.ShowToast);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form1";
            this.Text = "WinformsToastHost";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ShowToast;
        private System.Windows.Forms.TextBox textblockPackageId;
        private System.Windows.Forms.TextBox textblockPackagePath;
        private System.Windows.Forms.Button LoadAssembly;
        private System.Windows.Forms.TextBox Message;
    }
}

