namespace KojtoCAD.KojtoCAD3D
{
    partial class Form_Arrays_of_named_points
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
            this.comboBox_Names_List = new System.Windows.Forms.ComboBox();
            this.label_Names_Array = new System.Windows.Forms.Label();
            this.helpProvider_Arrays_of_named_points = new System.Windows.Forms.HelpProvider();
            this.button_Help = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_Names_List
            // 
            this.comboBox_Names_List.FormattingEnabled = true;
            this.comboBox_Names_List.Location = new System.Drawing.Point(31, 58);
            this.comboBox_Names_List.Name = "comboBox_Names_List";
            this.comboBox_Names_List.Size = new System.Drawing.Size(237, 21);
            this.comboBox_Names_List.TabIndex = 0;
            // 
            // label_Names_Array
            // 
            this.label_Names_Array.AutoSize = true;
            this.label_Names_Array.Location = new System.Drawing.Point(31, 39);
            this.label_Names_Array.Name = "label_Names_Array";
            this.label_Names_Array.Size = new System.Drawing.Size(59, 13);
            this.label_Names_Array.TabIndex = 1;
            this.label_Names_Array.Text = "Names List";
            // 
            // helpProvider_Arrays_of_named_points
            // 
            this.helpProvider_Arrays_of_named_points.HelpNamespace = "http://3dsoft.blob.core.windows.net/kojtocad/html/NAMED_POINTS_DRAW.htm";
            // 
            // button_Help
            // 
            this.button_Help.Location = new System.Drawing.Point(13, 234);
            this.button_Help.Name = "button_Help";
            this.button_Help.Size = new System.Drawing.Size(51, 23);
            this.button_Help.TabIndex = 2;
            this.button_Help.Text = "HELP";
            this.button_Help.UseVisualStyleBackColor = true;
            this.button_Help.Click += new System.EventHandler(this.button_Help_Click);
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(138, 234);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(52, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(205, 234);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(63, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // Form_Arrays_of_named_points
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 269);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Help);
            this.Controls.Add(this.label_Names_Array);
            this.Controls.Add(this.comboBox_Names_List);
            this.helpProvider_Arrays_of_named_points.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Name = "Form_Arrays_of_named_points";
            this.helpProvider_Arrays_of_named_points.SetShowHelp(this, true);
            this.Text = "Arrays of named points";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_Names_List;
        private System.Windows.Forms.Label label_Names_Array;
        private System.Windows.Forms.HelpProvider helpProvider_Arrays_of_named_points;
        private System.Windows.Forms.Button button_Help;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}