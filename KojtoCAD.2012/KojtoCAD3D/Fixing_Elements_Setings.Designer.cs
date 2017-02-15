namespace KojtoCAD.KojtoCAD3D
{
    partial class Fixing_Elements_Setings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Fixing_Elements_Setings));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_A = new System.Windows.Forms.TextBox();
            this.label_A = new System.Windows.Forms.Label();
            this.label_B = new System.Windows.Forms.Label();
            this.textBox_B = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.helpProvider_Fixing = new System.Windows.Forms.HelpProvider();
            this.button_HELP = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(264, 160);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 191);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "L - Bend Length";
            // 
            // textBox_A
            // 
            this.textBox_A.Location = new System.Drawing.Point(373, 13);
            this.textBox_A.Name = "textBox_A";
            this.textBox_A.Size = new System.Drawing.Size(100, 20);
            this.textBox_A.TabIndex = 2;
            // 
            // label_A
            // 
            this.label_A.AutoSize = true;
            this.label_A.Location = new System.Drawing.Point(338, 17);
            this.label_A.Name = "label_A";
            this.label_A.Size = new System.Drawing.Size(26, 13);
            this.label_A.TabIndex = 3;
            this.label_A.Text = "A = ";
            // 
            // label_B
            // 
            this.label_B.AutoSize = true;
            this.label_B.Location = new System.Drawing.Point(338, 53);
            this.label_B.Name = "label_B";
            this.label_B.Size = new System.Drawing.Size(26, 13);
            this.label_B.TabIndex = 5;
            this.label_B.Text = "B = ";
            // 
            // textBox_B
            // 
            this.textBox_B.Location = new System.Drawing.Point(373, 49);
            this.textBox_B.Name = "textBox_B";
            this.textBox_B.Size = new System.Drawing.Size(100, 20);
            this.textBox_B.TabIndex = 4;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(341, 119);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(438, 119);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // helpProvider_Fixing
            // 
            this.helpProvider_Fixing.HelpNamespace = "http://3dsoft.blob.core.windows.net/kojtocad/html/FIXING_SETTINGS.htm";
            // 
            // button_HELP
            // 
            this.button_HELP.Location = new System.Drawing.Point(202, 179);
            this.button_HELP.Name = "button_HELP";
            this.button_HELP.Size = new System.Drawing.Size(75, 23);
            this.button_HELP.TabIndex = 8;
            this.button_HELP.Text = "HELP";
            this.button_HELP.UseVisualStyleBackColor = true;
            this.button_HELP.Click += new System.EventHandler(this.button_HELP_Click);
            // 
            // Fixing_Elements_Setings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(581, 218);
            this.Controls.Add(this.button_HELP);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_B);
            this.Controls.Add(this.textBox_B);
            this.Controls.Add(this.label_A);
            this.Controls.Add(this.textBox_A);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.helpProvider_Fixing.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Name = "Fixing_Elements_Setings";
            this.helpProvider_Fixing.SetShowHelp(this, true);
            this.Text = "Fixing_Elements_Setings";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_A;
        private System.Windows.Forms.Label label_A;
        private System.Windows.Forms.Label label_B;
        private System.Windows.Forms.TextBox textBox_B;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.HelpProvider helpProvider_Fixing;
        private System.Windows.Forms.Button button_HELP;
    }
}