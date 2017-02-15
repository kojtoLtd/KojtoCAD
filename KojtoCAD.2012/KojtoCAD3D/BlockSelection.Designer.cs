namespace KojtoCAD.KojtoCAD3D
{
    partial class BlockSelection
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
            this.button_OK = new System.Windows.Forms.Button();
            this.comboBox_Blocks = new System.Windows.Forms.ComboBox();
            this.label_Blocks = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(217, 124);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // comboBox_Blocks
            // 
            this.comboBox_Blocks.FormattingEnabled = true;
            this.comboBox_Blocks.Location = new System.Drawing.Point(13, 28);
            this.comboBox_Blocks.Name = "comboBox_Blocks";
            this.comboBox_Blocks.Size = new System.Drawing.Size(197, 21);
            this.comboBox_Blocks.TabIndex = 1;
            // 
            // label_Blocks
            // 
            this.label_Blocks.AutoSize = true;
            this.label_Blocks.Location = new System.Drawing.Point(13, 9);
            this.label_Blocks.Name = "label_Blocks";
            this.label_Blocks.Size = new System.Drawing.Size(58, 13);
            this.label_Blocks.TabIndex = 2;
            this.label_Blocks.Text = "Blocks List";
            // 
            // BlockSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 159);
            this.Controls.Add(this.label_Blocks);
            this.Controls.Add(this.comboBox_Blocks);
            this.Controls.Add(this.button_OK);
            this.Name = "BlockSelection";
            this.Text = "BlockSelection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ComboBox comboBox_Blocks;
        private System.Windows.Forms.Label label_Blocks;
    }
}