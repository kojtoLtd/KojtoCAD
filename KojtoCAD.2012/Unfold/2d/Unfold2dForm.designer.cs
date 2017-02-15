namespace KojtoCAD.Unfold._2d
{
  partial class Unfold2dForm
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
            System.Windows.Forms.Panel panel;
            this.radioButtonMethodIsKFactor = new System.Windows.Forms.RadioButton();
            this.radioButtonMethodIsBendDeduction = new System.Windows.Forms.RadioButton();
            this.textBoxBendDeductionTableFile = new System.Windows.Forms.TextBox();
            this.buttonSelectBendDeductionTableFile = new System.Windows.Forms.Button();
            this.numericUpDownThickness = new System.Windows.Forms.NumericUpDown();
            this.labelORSegment = new System.Windows.Forms.Label();
            this.buttonSelectThicknessFromSegment = new System.Windows.Forms.Button();
            this.groupBoxNEthod = new System.Windows.Forms.GroupBox();
            this.textBoxKFactorTableFile = new System.Windows.Forms.TextBox();
            this.buttonSelectKFactorTableFile = new System.Windows.Forms.Button();
            this.radioButtonKFactorTable = new System.Windows.Forms.RadioButton();
            this.radioButtonKFactorUniform = new System.Windows.Forms.RadioButton();
            this.numericUpDownKFactor = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownInternalRadius = new System.Windows.Forms.NumericUpDown();
            this.comboBoxCoatingColor = new colorComboboxControl.ColorComboBox();
            this.labelCoatingColor = new System.Windows.Forms.Label();
            this.labelCoatingLayer = new System.Windows.Forms.Label();
            this.comboBoxCoatingLayer = new System.Windows.Forms.ComboBox();
            this.checkBoxDrawCoating = new System.Windows.Forms.CheckBox();
            this.groupBoxCoating = new System.Windows.Forms.GroupBox();
            this.radioButtonCoatingRight = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingBoth = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingLeft = new System.Windows.Forms.RadioButton();
            this.comboBoxDimensionColor = new colorComboboxControl.ColorComboBox();
            this.comboBoxDimensionLayer = new System.Windows.Forms.ComboBox();
            this.labelDimensionColor = new System.Windows.Forms.Label();
            this.comboBoxDimensionStyle = new System.Windows.Forms.ComboBox();
            this.labelDimensionLayer = new System.Windows.Forms.Label();
            this.labelDimensionStyle = new System.Windows.Forms.Label();
            this.checkBoxDrawDimensions = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxSheetThicknessConst = new System.Windows.Forms.GroupBox();
            this.groupBoxInternalRadius = new System.Windows.Forms.GroupBox();
            this.Ofd = new System.Windows.Forms.OpenFileDialog();
            panel = new System.Windows.Forms.Panel();
            panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThickness)).BeginInit();
            this.groupBoxNEthod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInternalRadius)).BeginInit();
            this.groupBoxCoating.SuspendLayout();
            this.groupBoxSheetThicknessConst.SuspendLayout();
            this.groupBoxInternalRadius.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            panel.Controls.Add(this.radioButtonMethodIsKFactor);
            panel.Controls.Add(this.radioButtonMethodIsBendDeduction);
            panel.Controls.Add(this.textBoxBendDeductionTableFile);
            panel.Controls.Add(this.buttonSelectBendDeductionTableFile);
            panel.Location = new System.Drawing.Point(3, 20);
            panel.Name = "panel";
            panel.Size = new System.Drawing.Size(196, 77);
            panel.TabIndex = 54;
            // 
            // radioButtonMethodIsKFactor
            // 
            this.radioButtonMethodIsKFactor.AutoSize = true;
            this.radioButtonMethodIsKFactor.Location = new System.Drawing.Point(3, 57);
            this.radioButtonMethodIsKFactor.Name = "radioButtonMethodIsKFactor";
            this.radioButtonMethodIsKFactor.Size = new System.Drawing.Size(62, 17);
            this.radioButtonMethodIsKFactor.TabIndex = 49;
            this.radioButtonMethodIsKFactor.TabStop = true;
            this.radioButtonMethodIsKFactor.Text = "K-factor";
            this.radioButtonMethodIsKFactor.UseVisualStyleBackColor = true;
            this.radioButtonMethodIsKFactor.CheckedChanged += new System.EventHandler(this.radioButtonMethodIsKFactor_CheckedChanged);
            // 
            // radioButtonMethodIsBendDeduction
            // 
            this.radioButtonMethodIsBendDeduction.AutoSize = true;
            this.radioButtonMethodIsBendDeduction.Location = new System.Drawing.Point(0, 3);
            this.radioButtonMethodIsBendDeduction.Name = "radioButtonMethodIsBendDeduction";
            this.radioButtonMethodIsBendDeduction.Size = new System.Drawing.Size(126, 17);
            this.radioButtonMethodIsBendDeduction.TabIndex = 0;
            this.radioButtonMethodIsBendDeduction.TabStop = true;
            this.radioButtonMethodIsBendDeduction.Text = "Bend deduction table";
            this.radioButtonMethodIsBendDeduction.UseVisualStyleBackColor = true;
            this.radioButtonMethodIsBendDeduction.CheckedChanged += new System.EventHandler(this.radioButtonMethodIsBendDeduction_CheckedChanged);
            // 
            // textBoxBendDeductionTableFile
            // 
            this.textBoxBendDeductionTableFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBendDeductionTableFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBendDeductionTableFile.Location = new System.Drawing.Point(16, 24);
            this.textBoxBendDeductionTableFile.Name = "textBoxBendDeductionTableFile";
            this.textBoxBendDeductionTableFile.Size = new System.Drawing.Size(133, 20);
            this.textBoxBendDeductionTableFile.TabIndex = 47;
            this.textBoxBendDeductionTableFile.TextChanged += new System.EventHandler(this.textBoxBendDeductionTableFile_TextChanged);
            // 
            // buttonSelectBendDeductionTableFile
            // 
            this.buttonSelectBendDeductionTableFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectBendDeductionTableFile.Location = new System.Drawing.Point(155, 24);
            this.buttonSelectBendDeductionTableFile.Name = "buttonSelectBendDeductionTableFile";
            this.buttonSelectBendDeductionTableFile.Size = new System.Drawing.Size(35, 22);
            this.buttonSelectBendDeductionTableFile.TabIndex = 48;
            this.buttonSelectBendDeductionTableFile.Text = "...";
            this.buttonSelectBendDeductionTableFile.UseVisualStyleBackColor = true;
            this.buttonSelectBendDeductionTableFile.Click += new System.EventHandler(this.buttonSelectBendDeductionTableFile_Click);
            // 
            // numericUpDownThickness
            // 
            this.numericUpDownThickness.DecimalPlaces = 2;
            this.numericUpDownThickness.Enabled = false;
            this.numericUpDownThickness.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownThickness.Location = new System.Drawing.Point(27, 19);
            this.numericUpDownThickness.Name = "numericUpDownThickness";
            this.numericUpDownThickness.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownThickness.TabIndex = 39;
            this.numericUpDownThickness.ThousandsSeparator = true;
            this.numericUpDownThickness.ValueChanged += new System.EventHandler(this.numericUpDownThickness_ValueChanged);
            // 
            // labelORSegment
            // 
            this.labelORSegment.AutoSize = true;
            this.labelORSegment.Location = new System.Drawing.Point(80, 21);
            this.labelORSegment.Name = "labelORSegment";
            this.labelORSegment.Size = new System.Drawing.Size(16, 13);
            this.labelORSegment.TabIndex = 40;
            this.labelORSegment.Text = "or";
            // 
            // buttonSelectThicknessFromSegment
            // 
            this.buttonSelectThicknessFromSegment.Location = new System.Drawing.Point(102, 16);
            this.buttonSelectThicknessFromSegment.Name = "buttonSelectThicknessFromSegment";
            this.buttonSelectThicknessFromSegment.Size = new System.Drawing.Size(91, 23);
            this.buttonSelectThicknessFromSegment.TabIndex = 41;
            this.buttonSelectThicknessFromSegment.Text = "Reverse";
            this.buttonSelectThicknessFromSegment.UseVisualStyleBackColor = true;
            this.buttonSelectThicknessFromSegment.Click += new System.EventHandler(this.buttonSelectThicknessFromSegment_Click);
            // 
            // groupBoxNEthod
            // 
            this.groupBoxNEthod.Controls.Add(panel);
            this.groupBoxNEthod.Controls.Add(this.textBoxKFactorTableFile);
            this.groupBoxNEthod.Controls.Add(this.buttonSelectKFactorTableFile);
            this.groupBoxNEthod.Controls.Add(this.radioButtonKFactorTable);
            this.groupBoxNEthod.Controls.Add(this.radioButtonKFactorUniform);
            this.groupBoxNEthod.Controls.Add(this.numericUpDownKFactor);
            this.groupBoxNEthod.Location = new System.Drawing.Point(5, 118);
            this.groupBoxNEthod.Name = "groupBoxNEthod";
            this.groupBoxNEthod.Size = new System.Drawing.Size(199, 181);
            this.groupBoxNEthod.TabIndex = 42;
            this.groupBoxNEthod.TabStop = false;
            this.groupBoxNEthod.Text = "Method";
            // 
            // textBoxKFactorTableFile
            // 
            this.textBoxKFactorTableFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxKFactorTableFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxKFactorTableFile.Location = new System.Drawing.Point(19, 150);
            this.textBoxKFactorTableFile.Name = "textBoxKFactorTableFile";
            this.textBoxKFactorTableFile.Size = new System.Drawing.Size(133, 20);
            this.textBoxKFactorTableFile.TabIndex = 52;
            this.textBoxKFactorTableFile.TextChanged += new System.EventHandler(this.textBoxKFactorTableFile_TextChanged);
            // 
            // buttonSelectKFactorTableFile
            // 
            this.buttonSelectKFactorTableFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectKFactorTableFile.Location = new System.Drawing.Point(158, 148);
            this.buttonSelectKFactorTableFile.Name = "buttonSelectKFactorTableFile";
            this.buttonSelectKFactorTableFile.Size = new System.Drawing.Size(35, 22);
            this.buttonSelectKFactorTableFile.TabIndex = 53;
            this.buttonSelectKFactorTableFile.Text = "...";
            this.buttonSelectKFactorTableFile.UseVisualStyleBackColor = true;
            this.buttonSelectKFactorTableFile.Click += new System.EventHandler(this.buttonSelectKFactorTableFile_Click);
            // 
            // radioButtonKFactorTable
            // 
            this.radioButtonKFactorTable.AutoSize = true;
            this.radioButtonKFactorTable.Location = new System.Drawing.Point(31, 127);
            this.radioButtonKFactorTable.Name = "radioButtonKFactorTable";
            this.radioButtonKFactorTable.Size = new System.Drawing.Size(91, 17);
            this.radioButtonKFactorTable.TabIndex = 51;
            this.radioButtonKFactorTable.TabStop = true;
            this.radioButtonKFactorTable.Text = "K-Factor table";
            this.radioButtonKFactorTable.UseVisualStyleBackColor = true;
            this.radioButtonKFactorTable.CheckedChanged += new System.EventHandler(this.radioButtonKFactorTable_CheckedChanged);
            // 
            // radioButtonKFactorUniform
            // 
            this.radioButtonKFactorUniform.AutoSize = true;
            this.radioButtonKFactorUniform.Location = new System.Drawing.Point(30, 104);
            this.radioButtonKFactorUniform.Name = "radioButtonKFactorUniform";
            this.radioButtonKFactorUniform.Size = new System.Drawing.Size(61, 17);
            this.radioButtonKFactorUniform.TabIndex = 50;
            this.radioButtonKFactorUniform.TabStop = true;
            this.radioButtonKFactorUniform.Text = "Uniform";
            this.radioButtonKFactorUniform.UseVisualStyleBackColor = true;
            this.radioButtonKFactorUniform.CheckedChanged += new System.EventHandler(this.radioButtonKFactorUniform_CheckedChanged);
            // 
            // numericUpDownKFactor
            // 
            this.numericUpDownKFactor.DecimalPlaces = 2;
            this.numericUpDownKFactor.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDownKFactor.Location = new System.Drawing.Point(97, 104);
            this.numericUpDownKFactor.Name = "numericUpDownKFactor";
            this.numericUpDownKFactor.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownKFactor.TabIndex = 45;
            this.numericUpDownKFactor.ThousandsSeparator = true;
            this.numericUpDownKFactor.ValueChanged += new System.EventHandler(this.numericUpDownKFactor_ValueChanged);
            // 
            // numericUpDownInternalRadius
            // 
            this.numericUpDownInternalRadius.DecimalPlaces = 2;
            this.numericUpDownInternalRadius.Enabled = false;
            this.numericUpDownInternalRadius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownInternalRadius.Location = new System.Drawing.Point(27, 19);
            this.numericUpDownInternalRadius.Name = "numericUpDownInternalRadius";
            this.numericUpDownInternalRadius.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownInternalRadius.TabIndex = 44;
            this.numericUpDownInternalRadius.ThousandsSeparator = true;
            this.numericUpDownInternalRadius.ValueChanged += new System.EventHandler(this.numericUpDownInternalRadius_ValueChanged);
            // 
            // comboBoxCoatingColor
            // 
            this.comboBoxCoatingColor.FormattingEnabled = true;
            this.comboBoxCoatingColor.IncludeSystemColors = true;
            this.comboBoxCoatingColor.Location = new System.Drawing.Point(99, 403);
            this.comboBoxCoatingColor.Name = "comboBoxCoatingColor";
            this.comboBoxCoatingColor.Size = new System.Drawing.Size(106, 21);
            this.comboBoxCoatingColor.SortAlphabetically = true;
            this.comboBoxCoatingColor.TabIndex = 63;
            this.comboBoxCoatingColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingColor_SelectedIndexChanged);
            // 
            // labelCoatingColor
            // 
            this.labelCoatingColor.AutoSize = true;
            this.labelCoatingColor.Location = new System.Drawing.Point(8, 406);
            this.labelCoatingColor.Name = "labelCoatingColor";
            this.labelCoatingColor.Size = new System.Drawing.Size(88, 13);
            this.labelCoatingColor.TabIndex = 62;
            this.labelCoatingColor.Text = "Coating line color";
            // 
            // labelCoatingLayer
            // 
            this.labelCoatingLayer.AutoSize = true;
            this.labelCoatingLayer.Location = new System.Drawing.Point(8, 379);
            this.labelCoatingLayer.Name = "labelCoatingLayer";
            this.labelCoatingLayer.Size = new System.Drawing.Size(87, 13);
            this.labelCoatingLayer.TabIndex = 61;
            this.labelCoatingLayer.Text = "Coating line layer";
            // 
            // comboBoxCoatingLayer
            // 
            this.comboBoxCoatingLayer.FormattingEnabled = true;
            this.comboBoxCoatingLayer.Location = new System.Drawing.Point(99, 376);
            this.comboBoxCoatingLayer.Name = "comboBoxCoatingLayer";
            this.comboBoxCoatingLayer.Size = new System.Drawing.Size(106, 21);
            this.comboBoxCoatingLayer.TabIndex = 60;
            this.comboBoxCoatingLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingLayer_SelectedIndexChanged);
            // 
            // checkBoxDrawCoating
            // 
            this.checkBoxDrawCoating.AutoSize = true;
            this.checkBoxDrawCoating.Location = new System.Drawing.Point(11, 305);
            this.checkBoxDrawCoating.Name = "checkBoxDrawCoating";
            this.checkBoxDrawCoating.Size = new System.Drawing.Size(102, 17);
            this.checkBoxDrawCoating.TabIndex = 59;
            this.checkBoxDrawCoating.Text = "Add coating line";
            this.checkBoxDrawCoating.UseVisualStyleBackColor = true;
            this.checkBoxDrawCoating.CheckedChanged += new System.EventHandler(this.checkBoxDrawCoating_CheckedChanged);
            // 
            // groupBoxCoating
            // 
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingRight);
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingBoth);
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingLeft);
            this.groupBoxCoating.Location = new System.Drawing.Point(11, 326);
            this.groupBoxCoating.Name = "groupBoxCoating";
            this.groupBoxCoating.Size = new System.Drawing.Size(194, 46);
            this.groupBoxCoating.TabIndex = 58;
            this.groupBoxCoating.TabStop = false;
            this.groupBoxCoating.Text = "Location";
            // 
            // radioButtonCoatingRight
            // 
            this.radioButtonCoatingRight.AutoSize = true;
            this.radioButtonCoatingRight.Location = new System.Drawing.Point(138, 19);
            this.radioButtonCoatingRight.Name = "radioButtonCoatingRight";
            this.radioButtonCoatingRight.Size = new System.Drawing.Size(50, 17);
            this.radioButtonCoatingRight.TabIndex = 38;
            this.radioButtonCoatingRight.TabStop = true;
            this.radioButtonCoatingRight.Text = "Right";
            this.radioButtonCoatingRight.UseVisualStyleBackColor = true;
            this.radioButtonCoatingRight.CheckedChanged += new System.EventHandler(this.radioButtonCoatingRight_CheckedChanged);
            // 
            // radioButtonCoatingBoth
            // 
            this.radioButtonCoatingBoth.AutoSize = true;
            this.radioButtonCoatingBoth.Location = new System.Drawing.Point(73, 19);
            this.radioButtonCoatingBoth.Name = "radioButtonCoatingBoth";
            this.radioButtonCoatingBoth.Size = new System.Drawing.Size(47, 17);
            this.radioButtonCoatingBoth.TabIndex = 37;
            this.radioButtonCoatingBoth.TabStop = true;
            this.radioButtonCoatingBoth.Text = "Both";
            this.radioButtonCoatingBoth.UseVisualStyleBackColor = true;
            this.radioButtonCoatingBoth.CheckedChanged += new System.EventHandler(this.radioButtonCoatingBoth_CheckedChanged);
            // 
            // radioButtonCoatingLeft
            // 
            this.radioButtonCoatingLeft.AutoSize = true;
            this.radioButtonCoatingLeft.Location = new System.Drawing.Point(7, 19);
            this.radioButtonCoatingLeft.Name = "radioButtonCoatingLeft";
            this.radioButtonCoatingLeft.Size = new System.Drawing.Size(43, 17);
            this.radioButtonCoatingLeft.TabIndex = 36;
            this.radioButtonCoatingLeft.TabStop = true;
            this.radioButtonCoatingLeft.Text = "Left";
            this.radioButtonCoatingLeft.UseVisualStyleBackColor = true;
            this.radioButtonCoatingLeft.CheckedChanged += new System.EventHandler(this.radioButtonCoatingLeft_CheckedChanged);
            // 
            // comboBoxDimensionColor
            // 
            this.comboBoxDimensionColor.FormattingEnabled = true;
            this.comboBoxDimensionColor.IncludeSystemColors = true;
            this.comboBoxDimensionColor.Location = new System.Drawing.Point(99, 521);
            this.comboBoxDimensionColor.Name = "comboBoxDimensionColor";
            this.comboBoxDimensionColor.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionColor.SortAlphabetically = true;
            this.comboBoxDimensionColor.TabIndex = 70;
            this.comboBoxDimensionColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionColor_SelectedIndexChanged);
            // 
            // comboBoxDimensionLayer
            // 
            this.comboBoxDimensionLayer.FormattingEnabled = true;
            this.comboBoxDimensionLayer.Location = new System.Drawing.Point(99, 495);
            this.comboBoxDimensionLayer.Name = "comboBoxDimensionLayer";
            this.comboBoxDimensionLayer.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionLayer.TabIndex = 67;
            this.comboBoxDimensionLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionLayer_SelectedIndexChanged);
            // 
            // labelDimensionColor
            // 
            this.labelDimensionColor.AutoSize = true;
            this.labelDimensionColor.Location = new System.Drawing.Point(8, 524);
            this.labelDimensionColor.Name = "labelDimensionColor";
            this.labelDimensionColor.Size = new System.Drawing.Size(82, 13);
            this.labelDimensionColor.TabIndex = 69;
            this.labelDimensionColor.Text = "Dimension color";
            // 
            // comboBoxDimensionStyle
            // 
            this.comboBoxDimensionStyle.FormattingEnabled = true;
            this.comboBoxDimensionStyle.Location = new System.Drawing.Point(99, 468);
            this.comboBoxDimensionStyle.Name = "comboBoxDimensionStyle";
            this.comboBoxDimensionStyle.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionStyle.TabIndex = 65;
            this.comboBoxDimensionStyle.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionStyle_SelectedIndexChanged);
            // 
            // labelDimensionLayer
            // 
            this.labelDimensionLayer.AutoSize = true;
            this.labelDimensionLayer.Location = new System.Drawing.Point(8, 498);
            this.labelDimensionLayer.Name = "labelDimensionLayer";
            this.labelDimensionLayer.Size = new System.Drawing.Size(81, 13);
            this.labelDimensionLayer.TabIndex = 68;
            this.labelDimensionLayer.Text = "Dimension layer";
            // 
            // labelDimensionStyle
            // 
            this.labelDimensionStyle.AutoSize = true;
            this.labelDimensionStyle.Location = new System.Drawing.Point(8, 471);
            this.labelDimensionStyle.Name = "labelDimensionStyle";
            this.labelDimensionStyle.Size = new System.Drawing.Size(80, 13);
            this.labelDimensionStyle.TabIndex = 66;
            this.labelDimensionStyle.Text = "Dimension style";
            // 
            // checkBoxDrawDimensions
            // 
            this.checkBoxDrawDimensions.AutoSize = true;
            this.checkBoxDrawDimensions.Location = new System.Drawing.Point(11, 445);
            this.checkBoxDrawDimensions.Name = "checkBoxDrawDimensions";
            this.checkBoxDrawDimensions.Size = new System.Drawing.Size(100, 17);
            this.checkBoxDrawDimensions.TabIndex = 64;
            this.checkBoxDrawDimensions.Text = "Add dimensions";
            this.checkBoxDrawDimensions.UseVisualStyleBackColor = true;
            this.checkBoxDrawDimensions.CheckedChanged += new System.EventHandler(this.checkBoxDrawDimensions_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(115, 560);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 23);
            this.buttonCancel.TabIndex = 72;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(11, 560);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 23);
            this.buttonOK.TabIndex = 71;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxSheetThicknessConst
            // 
            this.groupBoxSheetThicknessConst.Controls.Add(this.numericUpDownThickness);
            this.groupBoxSheetThicknessConst.Controls.Add(this.labelORSegment);
            this.groupBoxSheetThicknessConst.Controls.Add(this.buttonSelectThicknessFromSegment);
            this.groupBoxSheetThicknessConst.Location = new System.Drawing.Point(5, 7);
            this.groupBoxSheetThicknessConst.Name = "groupBoxSheetThicknessConst";
            this.groupBoxSheetThicknessConst.Size = new System.Drawing.Size(199, 48);
            this.groupBoxSheetThicknessConst.TabIndex = 74;
            this.groupBoxSheetThicknessConst.TabStop = false;
            this.groupBoxSheetThicknessConst.Text = "Sheet thickness";
            // 
            // groupBoxInternalRadius
            // 
            this.groupBoxInternalRadius.Controls.Add(this.numericUpDownInternalRadius);
            this.groupBoxInternalRadius.Location = new System.Drawing.Point(5, 61);
            this.groupBoxInternalRadius.Name = "groupBoxInternalRadius";
            this.groupBoxInternalRadius.Size = new System.Drawing.Size(199, 51);
            this.groupBoxInternalRadius.TabIndex = 75;
            this.groupBoxInternalRadius.TabStop = false;
            this.groupBoxInternalRadius.Text = "Internal bending radius";
            // 
            // Ofd
            // 
            this.Ofd.FileName = "Ofd";
            // 
            // Unfold2dForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 588);
            this.Controls.Add(this.groupBoxInternalRadius);
            this.Controls.Add(this.groupBoxSheetThicknessConst);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxDimensionColor);
            this.Controls.Add(this.comboBoxDimensionLayer);
            this.Controls.Add(this.labelDimensionColor);
            this.Controls.Add(this.comboBoxDimensionStyle);
            this.Controls.Add(this.labelDimensionLayer);
            this.Controls.Add(this.labelDimensionStyle);
            this.Controls.Add(this.checkBoxDrawDimensions);
            this.Controls.Add(this.comboBoxCoatingColor);
            this.Controls.Add(this.labelCoatingColor);
            this.Controls.Add(this.labelCoatingLayer);
            this.Controls.Add(this.comboBoxCoatingLayer);
            this.Controls.Add(this.checkBoxDrawCoating);
            this.Controls.Add(this.groupBoxCoating);
            this.Controls.Add(this.groupBoxNEthod);
            this.KeyPreview = true;
            this.Name = "Unfold2dForm";
            this.Text = "Unfold 2d Sheet Profile";
            this.Load += new System.EventHandler(this.Unfold2dForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Unfold2dForm_KeyPress);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThickness)).EndInit();
            this.groupBoxNEthod.ResumeLayout(false);
            this.groupBoxNEthod.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInternalRadius)).EndInit();
            this.groupBoxCoating.ResumeLayout(false);
            this.groupBoxCoating.PerformLayout();
            this.groupBoxSheetThicknessConst.ResumeLayout(false);
            this.groupBoxSheetThicknessConst.PerformLayout();
            this.groupBoxInternalRadius.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.NumericUpDown numericUpDownThickness;
    private System.Windows.Forms.Label labelORSegment;
    private System.Windows.Forms.Button buttonSelectThicknessFromSegment;
    private System.Windows.Forms.GroupBox groupBoxNEthod;
    private System.Windows.Forms.RadioButton radioButtonMethodIsBendDeduction;
    private System.Windows.Forms.NumericUpDown numericUpDownInternalRadius;
    private colorComboboxControl.ColorComboBox comboBoxCoatingColor;
    private System.Windows.Forms.Label labelCoatingColor;
    private System.Windows.Forms.Label labelCoatingLayer;
    private System.Windows.Forms.ComboBox comboBoxCoatingLayer;
    private System.Windows.Forms.CheckBox checkBoxDrawCoating;
    private System.Windows.Forms.GroupBox groupBoxCoating;
    private System.Windows.Forms.RadioButton radioButtonCoatingRight;
    private System.Windows.Forms.RadioButton radioButtonCoatingBoth;
    private System.Windows.Forms.RadioButton radioButtonCoatingLeft;
    private colorComboboxControl.ColorComboBox comboBoxDimensionColor;
    private System.Windows.Forms.ComboBox comboBoxDimensionLayer;
    private System.Windows.Forms.Label labelDimensionColor;
    private System.Windows.Forms.ComboBox comboBoxDimensionStyle;
    private System.Windows.Forms.Label labelDimensionLayer;
    private System.Windows.Forms.Label labelDimensionStyle;
    private System.Windows.Forms.CheckBox checkBoxDrawDimensions;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.GroupBox groupBoxSheetThicknessConst;
    private System.Windows.Forms.GroupBox groupBoxInternalRadius;
    private System.Windows.Forms.NumericUpDown numericUpDownKFactor;
    private System.Windows.Forms.TextBox textBoxBendDeductionTableFile;
    private System.Windows.Forms.Button buttonSelectBendDeductionTableFile;
    private System.Windows.Forms.OpenFileDialog Ofd;
    private System.Windows.Forms.TextBox textBoxKFactorTableFile;
    private System.Windows.Forms.Button buttonSelectKFactorTableFile;
    private System.Windows.Forms.RadioButton radioButtonKFactorTable;
    private System.Windows.Forms.RadioButton radioButtonKFactorUniform;
    private System.Windows.Forms.RadioButton radioButtonMethodIsKFactor;
  }
}