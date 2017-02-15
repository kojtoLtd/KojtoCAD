namespace KojtoCAD.KojtoCAD3D
{
    partial class Gaskets_FORM
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gaskets_FORM));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label_Gaskets_A = new System.Windows.Forms.Label();
            this.textBox_Gaskets_A = new System.Windows.Forms.TextBox();
            this.label_Gaskets_B = new System.Windows.Forms.Label();
            this.textBox_Gaskets_B = new System.Windows.Forms.TextBox();
            this.label_Gaskets_C = new System.Windows.Forms.Label();
            this.textBox_Gaskets_C = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_HELP = new System.Windows.Forms.Button();
            this.helpProvider_Gaskets = new System.Windows.Forms.HelpProvider();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(618, 329);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label_Gaskets_A
            // 
            this.label_Gaskets_A.AutoSize = true;
            this.label_Gaskets_A.Location = new System.Drawing.Point(17, 351);
            this.label_Gaskets_A.Name = "label_Gaskets_A";
            this.label_Gaskets_A.Size = new System.Drawing.Size(23, 13);
            this.label_Gaskets_A.TabIndex = 1;
            this.label_Gaskets_A.Text = "A =";
            // 
            // textBox_Gaskets_A
            // 
            this.textBox_Gaskets_A.Location = new System.Drawing.Point(47, 345);
            this.textBox_Gaskets_A.Name = "textBox_Gaskets_A";
            this.textBox_Gaskets_A.Size = new System.Drawing.Size(100, 20);
            this.textBox_Gaskets_A.TabIndex = 2;
            // 
            // label_Gaskets_B
            // 
            this.label_Gaskets_B.AutoSize = true;
            this.label_Gaskets_B.Location = new System.Drawing.Point(176, 351);
            this.label_Gaskets_B.Name = "label_Gaskets_B";
            this.label_Gaskets_B.Size = new System.Drawing.Size(26, 13);
            this.label_Gaskets_B.TabIndex = 3;
            this.label_Gaskets_B.Text = "B = ";
            // 
            // textBox_Gaskets_B
            // 
            this.textBox_Gaskets_B.Location = new System.Drawing.Point(202, 345);
            this.textBox_Gaskets_B.Name = "textBox_Gaskets_B";
            this.textBox_Gaskets_B.Size = new System.Drawing.Size(100, 20);
            this.textBox_Gaskets_B.TabIndex = 4;
            // 
            // label_Gaskets_C
            // 
            this.label_Gaskets_C.AutoSize = true;
            this.label_Gaskets_C.Location = new System.Drawing.Point(336, 351);
            this.label_Gaskets_C.Name = "label_Gaskets_C";
            this.label_Gaskets_C.Size = new System.Drawing.Size(26, 13);
            this.label_Gaskets_C.TabIndex = 5;
            this.label_Gaskets_C.Text = "C = ";
            // 
            // textBox_Gaskets_C
            // 
            this.textBox_Gaskets_C.Location = new System.Drawing.Point(366, 345);
            this.textBox_Gaskets_C.Name = "textBox_Gaskets_C";
            this.textBox_Gaskets_C.Size = new System.Drawing.Size(100, 20);
            this.textBox_Gaskets_C.TabIndex = 6;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(47, 390);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(202, 390);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_HELP
            // 
            this.button_HELP.Location = new System.Drawing.Point(359, 390);
            this.button_HELP.Name = "button_HELP";
            this.button_HELP.Size = new System.Drawing.Size(75, 23);
            this.button_HELP.TabIndex = 9;
            this.button_HELP.Text = "HELP";
            this.button_HELP.UseVisualStyleBackColor = true;
            this.button_HELP.Click += new System.EventHandler(this.button_HELP_Click);
            // 
            // helpProvider_Gaskets
            // 
            this.helpProvider_Gaskets.HelpNamespace = "http://3dsoft.blob.core.windows.net/kojtocad/html/Gaskets_Analize_Distances.htm";
            // 
            // Gaskets_FORM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 427);
            this.Controls.Add(this.button_HELP);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_Gaskets_C);
            this.Controls.Add(this.label_Gaskets_C);
            this.Controls.Add(this.textBox_Gaskets_B);
            this.Controls.Add(this.label_Gaskets_B);
            this.Controls.Add(this.textBox_Gaskets_A);
            this.Controls.Add(this.label_Gaskets_A);
            this.Controls.Add(this.pictureBox1);
            this.helpProvider_Gaskets.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Name = "Gaskets_FORM";
            this.helpProvider_Gaskets.SetShowHelp(this, true);
            this.Text = "Gaskets_FORM";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label_Gaskets_A;
        private System.Windows.Forms.TextBox textBox_Gaskets_A;
        private System.Windows.Forms.Label label_Gaskets_B;
        private System.Windows.Forms.TextBox textBox_Gaskets_B;
        private System.Windows.Forms.Label label_Gaskets_C;
        private System.Windows.Forms.TextBox textBox_Gaskets_C;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_HELP;
        private System.Windows.Forms.HelpProvider helpProvider_Gaskets;
    }
}