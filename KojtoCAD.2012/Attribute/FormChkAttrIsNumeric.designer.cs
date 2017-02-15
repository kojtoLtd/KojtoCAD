namespace KojtoCAD.Attribute
{
    partial class FormChkAttrIsNumeric
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxExistAttribute = new System.Windows.Forms.ComboBox();
            this.comboBoxNotExist = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxSheets = new System.Windows.Forms.ComboBox();
            this.comboBoxAttributes = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxBlocks = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxAllFiles = new System.Windows.Forms.CheckBox();
            this.buttonOutFolderBrowse = new System.Windows.Forms.Button();
            this.textBoxOutFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonExcelFileBrowse = new System.Windows.Forms.Button();
            this.textBoxExcelFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxRealNumber = new System.Windows.Forms.CheckBox();
            this.radioButtonDP = new System.Windows.Forms.RadioButton();
            this.radioButtonDC = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxSearchInXLSX = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxExistAttribute);
            this.groupBox1.Controls.Add(this.comboBoxNotExist);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(14, 279);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(519, 111);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Change attribute color to:  ";
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
            // comboBoxExistAttribute
            // 
            this.comboBoxExistAttribute.FormattingEnabled = true;
            this.comboBoxExistAttribute.Location = new System.Drawing.Point(15, 34);
            this.comboBoxExistAttribute.Name = "comboBoxExistAttribute";
            this.comboBoxExistAttribute.Size = new System.Drawing.Size(201, 21);
            this.comboBoxExistAttribute.TabIndex = 8;
            this.comboBoxExistAttribute.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxExistAttribute_DrawItem_1);
            // 
            // comboBoxNotExist
            // 
            this.comboBoxNotExist.FormattingEnabled = true;
            this.comboBoxNotExist.Location = new System.Drawing.Point(15, 66);
            this.comboBoxNotExist.Name = "comboBoxNotExist";
            this.comboBoxNotExist.Size = new System.Drawing.Size(201, 21);
            this.comboBoxNotExist.TabIndex = 10;
            this.comboBoxNotExist.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxNotExist_DrawItem_1);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(58, 193);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(145, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Select sheet of the .XLSX file";
            // 
            // comboBoxSheets
            // 
            this.comboBoxSheets.FormattingEnabled = true;
            this.comboBoxSheets.Location = new System.Drawing.Point(210, 190);
            this.comboBoxSheets.Name = "comboBoxSheets";
            this.comboBoxSheets.Size = new System.Drawing.Size(201, 21);
            this.comboBoxSheets.TabIndex = 33;
            // 
            // comboBoxAttributes
            // 
            this.comboBoxAttributes.FormattingEnabled = true;
            this.comboBoxAttributes.Location = new System.Drawing.Point(210, 246);
            this.comboBoxAttributes.Name = "comboBoxAttributes";
            this.comboBoxAttributes.Size = new System.Drawing.Size(201, 21);
            this.comboBoxAttributes.TabIndex = 32;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(79, 254);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Select Attribute (by TAG)";
            // 
            // comboBoxBlocks
            // 
            this.comboBoxBlocks.FormattingEnabled = true;
            this.comboBoxBlocks.Location = new System.Drawing.Point(210, 218);
            this.comboBoxBlocks.Name = "comboBoxBlocks";
            this.comboBoxBlocks.Size = new System.Drawing.Size(201, 21);
            this.comboBoxBlocks.TabIndex = 30;
            this.comboBoxBlocks.SelectedIndexChanged += new System.EventHandler(this.comboBoxBlocks_SelectedIndexChanged_1);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(136, 226);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Select Block";
            // 
            // checkBoxAllFiles
            // 
            this.checkBoxAllFiles.AutoSize = true;
            this.checkBoxAllFiles.Location = new System.Drawing.Point(231, 73);
            this.checkBoxAllFiles.Name = "checkBoxAllFiles";
            this.checkBoxAllFiles.Size = new System.Drawing.Size(279, 17);
            this.checkBoxAllFiles.TabIndex = 28;
            this.checkBoxAllFiles.Text = "Check Attributes in all DWG Files in the current Folder";
            this.checkBoxAllFiles.UseVisualStyleBackColor = true;
            // 
            // buttonOutFolderBrowse
            // 
            this.buttonOutFolderBrowse.Location = new System.Drawing.Point(487, 44);
            this.buttonOutFolderBrowse.Name = "buttonOutFolderBrowse";
            this.buttonOutFolderBrowse.Size = new System.Drawing.Size(31, 23);
            this.buttonOutFolderBrowse.TabIndex = 27;
            this.buttonOutFolderBrowse.Text = "...";
            this.buttonOutFolderBrowse.UseVisualStyleBackColor = true;
            this.buttonOutFolderBrowse.Click += new System.EventHandler(this.buttonOutFolderBrowse_Click_1);
            // 
            // textBoxOutFolder
            // 
            this.textBoxOutFolder.Location = new System.Drawing.Point(210, 43);
            this.textBoxOutFolder.Name = "textBoxOutFolder";
            this.textBoxOutFolder.Size = new System.Drawing.Size(271, 20);
            this.textBoxOutFolder.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(102, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Folder for Log File";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(367, 407);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click_1);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(458, 407);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 23;
            this.buttonOK.Text = "NEXT";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonExcelFileBrowse
            // 
            this.buttonExcelFileBrowse.Location = new System.Drawing.Point(487, 15);
            this.buttonExcelFileBrowse.Name = "buttonExcelFileBrowse";
            this.buttonExcelFileBrowse.Size = new System.Drawing.Size(31, 23);
            this.buttonExcelFileBrowse.TabIndex = 22;
            this.buttonExcelFileBrowse.Text = ",,,";
            this.buttonExcelFileBrowse.UseVisualStyleBackColor = true;
            this.buttonExcelFileBrowse.Click += new System.EventHandler(this.buttonExcelFileBrowse_Click_1);
            // 
            // textBoxExcelFile
            // 
            this.textBoxExcelFile.Location = new System.Drawing.Point(210, 17);
            this.textBoxExcelFile.Name = "textBoxExcelFile";
            this.textBoxExcelFile.Size = new System.Drawing.Size(271, 20);
            this.textBoxExcelFile.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Excel file to search attribute content";
            // 
            // checkBoxRealNumber
            // 
            this.checkBoxRealNumber.AutoSize = true;
            this.checkBoxRealNumber.Location = new System.Drawing.Point(22, 0);
            this.checkBoxRealNumber.Name = "checkBoxRealNumber";
            this.checkBoxRealNumber.Size = new System.Drawing.Size(214, 17);
            this.checkBoxRealNumber.TabIndex = 38;
            this.checkBoxRealNumber.Text = "Check that the attribute is a real number";
            this.checkBoxRealNumber.UseVisualStyleBackColor = true;
            this.checkBoxRealNumber.CheckedChanged += new System.EventHandler(this.checkBoxRealNumber_CheckedChanged);
            // 
            // radioButtonDP
            // 
            this.radioButtonDP.AutoSize = true;
            this.radioButtonDP.Location = new System.Drawing.Point(22, 29);
            this.radioButtonDP.Name = "radioButtonDP";
            this.radioButtonDP.Size = new System.Drawing.Size(98, 17);
            this.radioButtonDP.TabIndex = 41;
            this.radioButtonDP.TabStop = true;
            this.radioButtonDP.Text = "Delimiter - Point";
            this.radioButtonDP.UseVisualStyleBackColor = true;
            // 
            // radioButtonDC
            // 
            this.radioButtonDC.AutoSize = true;
            this.radioButtonDC.Location = new System.Drawing.Point(143, 29);
            this.radioButtonDC.Name = "radioButtonDC";
            this.radioButtonDC.Size = new System.Drawing.Size(109, 17);
            this.radioButtonDC.TabIndex = 40;
            this.radioButtonDC.TabStop = true;
            this.radioButtonDC.Text = "Delimiter - Comma";
            this.radioButtonDC.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxRealNumber);
            this.groupBox2.Controls.Add(this.radioButtonDC);
            this.groupBox2.Controls.Add(this.radioButtonDP);
            this.groupBox2.Location = new System.Drawing.Point(209, 121);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(272, 56);
            this.groupBox2.TabIndex = 39;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "                                                                           ";
            // 
            // checkBoxSearchInXLSX
            // 
            this.checkBoxSearchInXLSX.AutoSize = true;
            this.checkBoxSearchInXLSX.Location = new System.Drawing.Point(231, 97);
            this.checkBoxSearchInXLSX.Name = "checkBoxSearchInXLSX";
            this.checkBoxSearchInXLSX.Size = new System.Drawing.Size(101, 17);
            this.checkBoxSearchInXLSX.TabIndex = 40;
            this.checkBoxSearchInXLSX.Text = "Search in XLSX";
            this.checkBoxSearchInXLSX.UseVisualStyleBackColor = true;
            this.checkBoxSearchInXLSX.CheckedChanged += new System.EventHandler(this.checkBoxSearchInXLSX_CheckedChanged);
            // 
            // Form_ChkAttr_IsNumeric
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 466);
            this.Controls.Add(this.checkBoxSearchInXLSX);
            this.Controls.Add(this.groupBox2);
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
            this.Name = "Form_ChkAttr_IsNumeric";
            this.Text = "Form_ChkAttr_IsNumeric";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxExistAttribute;
        private System.Windows.Forms.ComboBox comboBoxNotExist;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxSheets;
        private System.Windows.Forms.ComboBox comboBoxAttributes;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxBlocks;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxAllFiles;
        private System.Windows.Forms.Button buttonOutFolderBrowse;
        private System.Windows.Forms.TextBox textBoxOutFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonExcelFileBrowse;
        private System.Windows.Forms.TextBox textBoxExcelFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxRealNumber;
        private System.Windows.Forms.RadioButton radioButtonDP;
        private System.Windows.Forms.RadioButton radioButtonDC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxSearchInXLSX;
    }
}