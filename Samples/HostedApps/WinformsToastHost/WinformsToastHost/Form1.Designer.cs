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
            this.btnShowToast = new System.Windows.Forms.Button();
            this.txtPackageId = new System.Windows.Forms.TextBox();
            this.txtPackagePath = new System.Windows.Forms.TextBox();
            this.btnRunHostedApp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(14, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Package Identity:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(14, 57);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Package Path:";
            // 
            // btnShowToast
            // 
            this.btnShowToast.Location = new System.Drawing.Point(14, 107);
            this.btnShowToast.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.btnShowToast.Name = "btnShowToast";
            this.btnShowToast.Size = new System.Drawing.Size(145, 34);
            this.btnShowToast.TabIndex = 2;
            this.btnShowToast.Text = "Show Toast";
            this.btnShowToast.UseVisualStyleBackColor = true;
            this.btnShowToast.Click += new System.EventHandler(this.btnShowToast_Click);
            // 
            // txtPackageId
            // 
            this.txtPackageId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPackageId.Location = new System.Drawing.Point(205, 6);
            this.txtPackageId.Name = "txtPackageId";
            this.txtPackageId.ReadOnly = true;
            this.txtPackageId.Size = new System.Drawing.Size(614, 35);
            this.txtPackageId.TabIndex = 3;
            // 
            // txtPackagePath
            // 
            this.txtPackagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPackagePath.Location = new System.Drawing.Point(205, 54);
            this.txtPackagePath.Name = "txtPackagePath";
            this.txtPackagePath.ReadOnly = true;
            this.txtPackagePath.Size = new System.Drawing.Size(614, 35);
            this.txtPackagePath.TabIndex = 4;
            // 
            // btnRunHostedApp
            // 
            this.btnRunHostedApp.Location = new System.Drawing.Point(171, 107);
            this.btnRunHostedApp.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.btnRunHostedApp.Name = "btnRunHostedApp";
            this.btnRunHostedApp.Size = new System.Drawing.Size(145, 34);
            this.btnRunHostedApp.TabIndex = 5;
            this.btnRunHostedApp.Text = "Run hosted app";
            this.btnRunHostedApp.UseVisualStyleBackColor = true;
            this.btnRunHostedApp.Click += new System.EventHandler(this.btnRunHostedApp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 154);
            this.Controls.Add(this.txtPackageId);
            this.Controls.Add(this.btnRunHostedApp);
            this.Controls.Add(this.txtPackagePath);
            this.Controls.Add(this.btnShowToast);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form1";
            this.Text = "WinForms Toast Host";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnShowToast;
        private System.Windows.Forms.TextBox txtPackageId;
        private System.Windows.Forms.TextBox txtPackagePath;
        private System.Windows.Forms.Button btnRunHostedApp;
    }
}

