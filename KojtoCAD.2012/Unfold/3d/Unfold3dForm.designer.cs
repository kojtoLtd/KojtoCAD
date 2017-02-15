namespace KojtoCAD.Unfold._3d
{
    partial class Unfold3dForm
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
            this.textBox_S = new System.Windows.Forms.TextBox();
            this.textBox_R = new System.Windows.Forms.TextBox();
            this.label_S = new System.Windows.Forms.Label();
            this.label_R = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_KFactor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonKFHelp = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButtonAirBending = new System.Windows.Forms.RadioButton();
            this.radioButtonBA = new System.Windows.Forms.RadioButton();
            this.radioButtonBD = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelKFactor = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_S
            // 
            this.textBox_S.Location = new System.Drawing.Point(127, 149);
            this.textBox_S.Name = "textBox_S";
            this.textBox_S.Size = new System.Drawing.Size(86, 20);
            this.textBox_S.TabIndex = 0;
            // 
            // textBox_R
            // 
            this.textBox_R.Location = new System.Drawing.Point(127, 175);
            this.textBox_R.Name = "textBox_R";
            this.textBox_R.Size = new System.Drawing.Size(90, 20);
            this.textBox_R.TabIndex = 1;
            // 
            // label_S
            // 
            this.label_S.AutoSize = true;
            this.label_S.Location = new System.Drawing.Point(9, 152);
            this.label_S.Name = "label_S";
            this.label_S.Size = new System.Drawing.Size(113, 13);
            this.label_S.TabIndex = 2;
            this.label_S.Text = "material thickness S = ";
            // 
            // label_R
            // 
            this.label_R.AutoSize = true;
            this.label_R.Location = new System.Drawing.Point(9, 178);
            this.label_R.Name = "label_R";
            this.label_R.Size = new System.Drawing.Size(92, 13);
            this.label_R.TabIndex = 3;
            this.label_R.Text = "internal radius R =";
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(20, 227);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(201, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_KFactor
            // 
            this.textBox_KFactor.Location = new System.Drawing.Point(127, 201);
            this.textBox_KFactor.Name = "textBox_KFactor";
            this.textBox_KFactor.Size = new System.Drawing.Size(90, 20);
            this.textBox_KFactor.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "K - Factor";
            // 
            // buttonKFHelp
            // 
            this.buttonKFHelp.Location = new System.Drawing.Point(157, 23);
            this.buttonKFHelp.Name = "buttonKFHelp";
            this.buttonKFHelp.Size = new System.Drawing.Size(28, 23);
            this.buttonKFHelp.TabIndex = 7;
            this.buttonKFHelp.Text = "?";
            this.buttonKFHelp.UseVisualStyleBackColor = true;
            this.buttonKFHelp.Click += new System.EventHandler(this.buttonKFHelp_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Bend Aloowance";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Bend Deduction";
            // 
            // radioButtonAirBending
            // 
            this.radioButtonAirBending.AutoSize = true;
            this.radioButtonAirBending.Location = new System.Drawing.Point(126, 28);
            this.radioButtonAirBending.Name = "radioButtonAirBending";
            this.radioButtonAirBending.Size = new System.Drawing.Size(14, 13);
            this.radioButtonAirBending.TabIndex = 12;
            this.radioButtonAirBending.TabStop = true;
            this.radioButtonAirBending.UseVisualStyleBackColor = true;
            this.radioButtonAirBending.CheckedChanged += new System.EventHandler(this.radioButtonKF_CheckedChanged);
            // 
            // radioButtonBA
            // 
            this.radioButtonBA.AutoSize = true;
            this.radioButtonBA.Location = new System.Drawing.Point(126, 53);
            this.radioButtonBA.Name = "radioButtonBA";
            this.radioButtonBA.Size = new System.Drawing.Size(14, 13);
            this.radioButtonBA.TabIndex = 13;
            this.radioButtonBA.TabStop = true;
            this.radioButtonBA.UseVisualStyleBackColor = true;
            this.radioButtonBA.CheckedChanged += new System.EventHandler(this.radioButtonBA_CheckedChanged);
            // 
            // radioButtonBD
            // 
            this.radioButtonBD.AutoSize = true;
            this.radioButtonBD.Location = new System.Drawing.Point(126, 85);
            this.radioButtonBD.Name = "radioButtonBD";
            this.radioButtonBD.Size = new System.Drawing.Size(14, 13);
            this.radioButtonBD.TabIndex = 14;
            this.radioButtonBD.TabStop = true;
            this.radioButtonBD.UseVisualStyleBackColor = true;
            this.radioButtonBD.CheckedChanged += new System.EventHandler(this.radioButtonBD_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "?";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(157, 78);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "?";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonKFHelp);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.radioButtonAirBending);
            this.groupBox1.Controls.Add(this.radioButtonBA);
            this.groupBox1.Controls.Add(this.radioButtonBD);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(201, 115);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " METHOD";
            // 
            // labelKFactor
            // 
            this.labelKFactor.AutoSize = true;
            this.labelKFactor.Location = new System.Drawing.Point(12, 201);
            this.labelKFactor.Name = "labelKFactor";
            this.labelKFactor.Size = new System.Drawing.Size(47, 13);
            this.labelKFactor.TabIndex = 29;
            this.labelKFactor.Text = "K-Factor";
            // 
            // Unfold3dForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(229, 261);
            this.Controls.Add(this.labelKFactor);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox_KFactor);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_R);
            this.Controls.Add(this.label_S);
            this.Controls.Add(this.textBox_R);
            this.Controls.Add(this.textBox_S);
            this.KeyPreview = true;
            this.Name = "Unfold3dForm";
            this.Text = "Import Unfold3D Data";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Unfold3dForm_KeyPress);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_S;
        private System.Windows.Forms.TextBox textBox_R;
        private System.Windows.Forms.Label label_S;
        private System.Windows.Forms.Label label_R;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_KFactor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonKFHelp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButtonAirBending;
        private System.Windows.Forms.RadioButton radioButtonBA;
      private System.Windows.Forms.RadioButton radioButtonBD;
        private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label labelKFactor;
    }
}