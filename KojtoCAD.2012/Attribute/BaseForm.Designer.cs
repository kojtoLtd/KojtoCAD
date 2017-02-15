namespace KojtoCAD.Attribute
{
    partial class BaseForm
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
            this.textBoxExcelFile = new System.Windows.Forms.TextBox();
            this.buttonExcelFileBrowse = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxOutFolder = new System.Windows.Forms.TextBox();
            this.buttonOutFolderBrowse = new System.Windows.Forms.Button();
            this.comboBoxExistAttribute = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxNotExist = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxAllFiles = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxBlocks = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxAttributes = new System.Windows.Forms.ComboBox();
            this.comboBoxSheets = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Excel file to search attribute content";
            // 
            // textBoxExcelFile
            // 
            this.textBoxExcelFile.Location = new System.Drawing.Point(196, 13);
            this.textBoxExcelFile.Name = "textBoxExcelFile";
            this.textBoxExcelFile.Size = new System.Drawing.Size(271, 20);
            this.textBoxExcelFile.TabIndex = 1;
            // 
            // buttonExcelFileBrowse
            // 
            this.buttonExcelFileBrowse.Location = new System.Drawing.Point(473, 11);
            this.buttonExcelFileBrowse.Name = "buttonExcelFileBrowse";
            this.buttonExcelFileBrowse.Size = new System.Drawing.Size(31, 23);
            this.buttonExcelFileBrowse.TabIndex = 2;
            this.buttonExcelFileBrowse.Text = ",,,";
            this.buttonExcelFileBrowse.UseVisualStyleBackColor = true;
            this.buttonExcelFileBrowse.Click += new System.EventHandler(this.buttonExcelFileBrowse_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(444, 323);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "NEXT";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(353, 323);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Folder for Log File";
            // 
            // textBoxOutFolder
            // 
            this.textBoxOutFolder.Location = new System.Drawing.Point(196, 39);
            this.textBoxOutFolder.Name = "textBoxOutFolder";
            this.textBoxOutFolder.Size = new System.Drawing.Size(271, 20);
            this.textBoxOutFolder.TabIndex = 6;
            // 
            // buttonOutFolderBrowse
            // 
            this.buttonOutFolderBrowse.Location = new System.Drawing.Point(473, 40);
            this.buttonOutFolderBrowse.Name = "buttonOutFolderBrowse";
            this.buttonOutFolderBrowse.Size = new System.Drawing.Size(31, 23);
            this.buttonOutFolderBrowse.TabIndex = 7;
            this.buttonOutFolderBrowse.Text = "...";
            this.buttonOutFolderBrowse.UseVisualStyleBackColor = true;
            this.buttonOutFolderBrowse.Click += new System.EventHandler(this.buttonOutFolderBrowse_Click);
            // 
            // comboBoxExistAttribute
            // 
            this.comboBoxExistAttribute.FormattingEnabled = true;
            this.comboBoxExistAttribute.Location = new System.Drawing.Point(15, 34);
            this.comboBoxExistAttribute.Name = "comboBoxExistAttribute";
            this.comboBoxExistAttribute.Size = new System.Drawing.Size(201, 21);
            this.comboBoxExistAttribute.TabIndex = 8;
            this.comboBoxExistAttribute.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxExistAttribute_DrawItem);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(257, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Attribute colour if it\'s content is found in the Excel File";
            // 
            // comboBoxNotExist
            // 
            this.comboBoxNotExist.FormattingEnabled = true;
            this.comboBoxNotExist.Location = new System.Drawing.Point(15, 66);
            this.comboBoxNotExist.Name = "comboBoxNotExist";
            this.comboBoxNotExist.Size = new System.Drawing.Size(201, 21);
            this.comboBoxNotExist.TabIndex = 10;
            this.comboBoxNotExist.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxNotExist_DrawItem);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(221, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(283, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Attribute colour if it\'s content is NOT found in the Excel File";
            // 
            // checkBoxAllFiles
            // 
            this.checkBoxAllFiles.AutoSize = true;
            this.checkBoxAllFiles.Location = new System.Drawing.Point(196, 69);
            this.checkBoxAllFiles.Name = "checkBoxAllFiles";
            this.checkBoxAllFiles.Size = new System.Drawing.Size(279, 17);
            this.checkBoxAllFiles.TabIndex = 12;
            this.checkBoxAllFiles.Text = "Check Attributes in all DWG Files in the current Folder";
            this.checkBoxAllFiles.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(122, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Select Block";
            // 
            // comboBoxBlocks
            // 
            this.comboBoxBlocks.FormattingEnabled = true;
            this.comboBoxBlocks.Location = new System.Drawing.Point(196, 126);
            this.comboBoxBlocks.Name = "comboBoxBlocks";
            this.comboBoxBlocks.Size = new System.Drawing.Size(201, 21);
            this.comboBoxBlocks.TabIndex = 14;
            this.comboBoxBlocks.SelectedIndexChanged += new System.EventHandler(this.comboBoxBlocks_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(65, 162);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Select Attribute (by TAG)";
            // 
            // comboBoxAttributes
            // 
            this.comboBoxAttributes.FormattingEnabled = true;
            this.comboBoxAttributes.Location = new System.Drawing.Point(196, 154);
            this.comboBoxAttributes.Name = "comboBoxAttributes";
            this.comboBoxAttributes.Size = new System.Drawing.Size(201, 21);
            this.comboBoxAttributes.TabIndex = 16;
            // 
            // comboBoxSheets
            // 
            this.comboBoxSheets.FormattingEnabled = true;
            this.comboBoxSheets.Location = new System.Drawing.Point(196, 98);
            this.comboBoxSheets.Name = "comboBoxSheets";
            this.comboBoxSheets.Size = new System.Drawing.Size(201, 21);
            this.comboBoxSheets.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 101);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(145, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Select sheet of the .XLSX file";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxExistAttribute);
            this.groupBox1.Controls.Add(this.comboBoxNotExist);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(0, 197);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(519, 111);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Change attribute color to:  ";
            // 
            // BaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 360);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.comboBoxSheets);
            this.Controls.Add(this.comboBoxAttributes);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxBlocks);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.checkBoxAllFiles);
            this.Controls.Add(this.buttonOutFolderBrowse);
            this.Controls.Add(this.textBoxOutFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonExcelFileBrowse);
            this.Controls.Add(this.textBoxExcelFile);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.Name = "BaseForm";
            this.Text = "Check if attribute content is foung in a *.XLSX file";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BaseForm_KeyPress);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxExcelFile;
        private System.Windows.Forms.Button buttonExcelFileBrowse;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxOutFolder;
        private System.Windows.Forms.Button buttonOutFolderBrowse;
        private System.Windows.Forms.ComboBox comboBoxExistAttribute;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxNotExist;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxAllFiles;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxBlocks;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxAttributes;
        private System.Windows.Forms.ComboBox comboBoxSheets;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}