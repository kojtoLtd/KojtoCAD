namespace KojtoCAD.GraphicItems.SheetProfile
{
  partial class SheetProfileGenerationForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SheetProfileGenerationForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelInternalRadius = new System.Windows.Forms.Label();
            this.labelSheetThickness = new System.Windows.Forms.Label();
            this.radioButtonMaterialSideLeft = new System.Windows.Forms.RadioButton();
            this.groupBoxSide = new System.Windows.Forms.GroupBox();
            this.radioButtonMaterialSideCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonMaterialSideRight = new System.Windows.Forms.RadioButton();
            this.groupBoxCoating = new System.Windows.Forms.GroupBox();
            this.radioButtonCoatingRight = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingBoth = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingLeft = new System.Windows.Forms.RadioButton();
            this.checkBoxDrawDimensions = new System.Windows.Forms.CheckBox();
            this.numericUpDownThickness = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxDrawCoating = new System.Windows.Forms.CheckBox();
            this.comboBoxMaterialLayer = new System.Windows.Forms.ComboBox();
            this.labelMaterialLayer = new System.Windows.Forms.Label();
            this.labelMaterialColor = new System.Windows.Forms.Label();
            this.labelCoatingColor = new System.Windows.Forms.Label();
            this.labelCoatingLayer = new System.Windows.Forms.Label();
            this.comboBoxCoatingLayer = new System.Windows.Forms.ComboBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.labelDimensionStyle = new System.Windows.Forms.Label();
            this.labelDimensionLayer = new System.Windows.Forms.Label();
            this.comboBoxDimensionStyle = new System.Windows.Forms.ComboBox();
            this.labelDimensionColor = new System.Windows.Forms.Label();
            this.comboBoxDimensionLayer = new System.Windows.Forms.ComboBox();
            this.radioButtonDimStandartArcLine = new System.Windows.Forms.RadioButton();
            this.radioButtonDimStandartProjections = new System.Windows.Forms.RadioButton();
            this.comboBoxCoatingColor = new colorComboboxControl.ColorComboBox();
            this.comboBoxMaterialColor = new colorComboboxControl.ColorComboBox();
            this.comboBoxDimensionColor = new colorComboboxControl.ColorComboBox();
            this.groupBoxDimensionLinesStandart = new System.Windows.Forms.GroupBox();
            this.numericUpDownInternalRadius = new System.Windows.Forms.NumericUpDown();
            this.groupBoxSide.SuspendLayout();
            this.groupBoxCoating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.groupBoxDimensionLinesStandart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInternalRadius)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(10, 455);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 30);
            this.buttonOK.TabIndex = 32;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelInternalRadius
            // 
            this.labelInternalRadius.AutoSize = true;
            this.labelInternalRadius.Location = new System.Drawing.Point(9, 32);
            this.labelInternalRadius.Name = "labelInternalRadius";
            this.labelInternalRadius.Size = new System.Drawing.Size(114, 13);
            this.labelInternalRadius.TabIndex = 31;
            this.labelInternalRadius.Text = "Internal bending radius";
            // 
            // labelSheetThickness
            // 
            this.labelSheetThickness.AutoSize = true;
            this.labelSheetThickness.Location = new System.Drawing.Point(9, 7);
            this.labelSheetThickness.Name = "labelSheetThickness";
            this.labelSheetThickness.Size = new System.Drawing.Size(83, 13);
            this.labelSheetThickness.TabIndex = 28;
            this.labelSheetThickness.Text = "Sheet thickness";
            // 
            // radioButtonMaterialSideLeft
            // 
            this.radioButtonMaterialSideLeft.AutoSize = true;
            this.radioButtonMaterialSideLeft.Location = new System.Drawing.Point(7, 19);
            this.radioButtonMaterialSideLeft.Name = "radioButtonMaterialSideLeft";
            this.radioButtonMaterialSideLeft.Size = new System.Drawing.Size(43, 17);
            this.radioButtonMaterialSideLeft.TabIndex = 33;
            this.radioButtonMaterialSideLeft.TabStop = true;
            this.radioButtonMaterialSideLeft.Text = "Left";
            this.radioButtonMaterialSideLeft.UseVisualStyleBackColor = true;
            this.radioButtonMaterialSideLeft.CheckedChanged += new System.EventHandler(this.radioButtonMaterialSideLeft_CheckedChanged);
            // 
            // groupBoxSide
            // 
            this.groupBoxSide.Controls.Add(this.radioButtonMaterialSideCenter);
            this.groupBoxSide.Controls.Add(this.radioButtonMaterialSideRight);
            this.groupBoxSide.Controls.Add(this.radioButtonMaterialSideLeft);
            this.groupBoxSide.Location = new System.Drawing.Point(10, 56);
            this.groupBoxSide.Name = "groupBoxSide";
            this.groupBoxSide.Size = new System.Drawing.Size(196, 46);
            this.groupBoxSide.TabIndex = 34;
            this.groupBoxSide.TabStop = false;
            this.groupBoxSide.Text = "Offset sheet";
            // 
            // radioButtonMaterialSideCenter
            // 
            this.radioButtonMaterialSideCenter.AutoSize = true;
            this.radioButtonMaterialSideCenter.Location = new System.Drawing.Point(75, 19);
            this.radioButtonMaterialSideCenter.Name = "radioButtonMaterialSideCenter";
            this.radioButtonMaterialSideCenter.Size = new System.Drawing.Size(56, 17);
            this.radioButtonMaterialSideCenter.TabIndex = 35;
            this.radioButtonMaterialSideCenter.TabStop = true;
            this.radioButtonMaterialSideCenter.Text = "Center";
            this.radioButtonMaterialSideCenter.UseVisualStyleBackColor = true;
            this.radioButtonMaterialSideCenter.CheckedChanged += new System.EventHandler(this.radioButtonMaterialSideCenter_CheckedChanged);
            // 
            // radioButtonMaterialSideRight
            // 
            this.radioButtonMaterialSideRight.AutoSize = true;
            this.radioButtonMaterialSideRight.Location = new System.Drawing.Point(140, 19);
            this.radioButtonMaterialSideRight.Name = "radioButtonMaterialSideRight";
            this.radioButtonMaterialSideRight.Size = new System.Drawing.Size(50, 17);
            this.radioButtonMaterialSideRight.TabIndex = 34;
            this.radioButtonMaterialSideRight.TabStop = true;
            this.radioButtonMaterialSideRight.Text = "Right";
            this.radioButtonMaterialSideRight.UseVisualStyleBackColor = true;
            this.radioButtonMaterialSideRight.CheckedChanged += new System.EventHandler(this.radioButtonMaterialSideRight_CheckedChanged);
            // 
            // groupBoxCoating
            // 
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingRight);
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingBoth);
            this.groupBoxCoating.Controls.Add(this.radioButtonCoatingLeft);
            this.groupBoxCoating.Location = new System.Drawing.Point(12, 189);
            this.groupBoxCoating.Name = "groupBoxCoating";
            this.groupBoxCoating.Size = new System.Drawing.Size(194, 46);
            this.groupBoxCoating.TabIndex = 35;
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
            // checkBoxDrawDimensions
            // 
            this.checkBoxDrawDimensions.AutoSize = true;
            this.checkBoxDrawDimensions.Location = new System.Drawing.Point(12, 303);
            this.checkBoxDrawDimensions.Name = "checkBoxDrawDimensions";
            this.checkBoxDrawDimensions.Size = new System.Drawing.Size(100, 17);
            this.checkBoxDrawDimensions.TabIndex = 36;
            this.checkBoxDrawDimensions.Text = "Add dimensions";
            this.checkBoxDrawDimensions.UseVisualStyleBackColor = true;
            this.checkBoxDrawDimensions.CheckedChanged += new System.EventHandler(this.checkBoxDimensions_CheckedChanged);
            // 
            // numericUpDownThickness
            // 
            this.numericUpDownThickness.DecimalPlaces = 2;
            this.numericUpDownThickness.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownThickness.Location = new System.Drawing.Point(129, 5);
            this.numericUpDownThickness.Name = "numericUpDownThickness";
            this.numericUpDownThickness.Size = new System.Drawing.Size(77, 20);
            this.numericUpDownThickness.TabIndex = 37;
            this.numericUpDownThickness.ThousandsSeparator = true;
            this.numericUpDownThickness.ValueChanged += new System.EventHandler(this.numericUpDownThickness_ValueChanged);
            this.numericUpDownThickness.Validating += new System.ComponentModel.CancelEventHandler(this.numericUpDownThickness_Validating);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(116, 455);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 30);
            this.buttonCancel.TabIndex = 38;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkBoxDrawCoating
            // 
            this.checkBoxDrawCoating.AutoSize = true;
            this.checkBoxDrawCoating.Location = new System.Drawing.Point(12, 168);
            this.checkBoxDrawCoating.Name = "checkBoxDrawCoating";
            this.checkBoxDrawCoating.Size = new System.Drawing.Size(102, 17);
            this.checkBoxDrawCoating.TabIndex = 40;
            this.checkBoxDrawCoating.Text = "Add coating line";
            this.checkBoxDrawCoating.UseVisualStyleBackColor = true;
            this.checkBoxDrawCoating.CheckedChanged += new System.EventHandler(this.checkBoxDrawCoating_CheckedChanged);
            // 
            // comboBoxMaterialLayer
            // 
            this.comboBoxMaterialLayer.FormattingEnabled = true;
            this.comboBoxMaterialLayer.Location = new System.Drawing.Point(100, 108);
            this.comboBoxMaterialLayer.Name = "comboBoxMaterialLayer";
            this.comboBoxMaterialLayer.Size = new System.Drawing.Size(106, 21);
            this.comboBoxMaterialLayer.TabIndex = 41;
            this.comboBoxMaterialLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxMaterialLayer_SelectedIndexChanged);
            // 
            // labelMaterialLayer
            // 
            this.labelMaterialLayer.AutoSize = true;
            this.labelMaterialLayer.Location = new System.Drawing.Point(9, 111);
            this.labelMaterialLayer.Name = "labelMaterialLayer";
            this.labelMaterialLayer.Size = new System.Drawing.Size(60, 13);
            this.labelMaterialLayer.TabIndex = 43;
            this.labelMaterialLayer.Text = "Sheet layer";
            // 
            // labelMaterialColor
            // 
            this.labelMaterialColor.AutoSize = true;
            this.labelMaterialColor.Location = new System.Drawing.Point(9, 137);
            this.labelMaterialColor.Name = "labelMaterialColor";
            this.labelMaterialColor.Size = new System.Drawing.Size(61, 13);
            this.labelMaterialColor.TabIndex = 44;
            this.labelMaterialColor.Text = "Sheet color";
            // 
            // labelCoatingColor
            // 
            this.labelCoatingColor.AutoSize = true;
            this.labelCoatingColor.Location = new System.Drawing.Point(9, 269);
            this.labelCoatingColor.Name = "labelCoatingColor";
            this.labelCoatingColor.Size = new System.Drawing.Size(88, 13);
            this.labelCoatingColor.TabIndex = 48;
            this.labelCoatingColor.Text = "Coating line color";
            // 
            // labelCoatingLayer
            // 
            this.labelCoatingLayer.AutoSize = true;
            this.labelCoatingLayer.Location = new System.Drawing.Point(9, 242);
            this.labelCoatingLayer.Name = "labelCoatingLayer";
            this.labelCoatingLayer.Size = new System.Drawing.Size(87, 13);
            this.labelCoatingLayer.TabIndex = 47;
            this.labelCoatingLayer.Text = "Coating line layer";
            // 
            // comboBoxCoatingLayer
            // 
            this.comboBoxCoatingLayer.FormattingEnabled = true;
            this.comboBoxCoatingLayer.Location = new System.Drawing.Point(100, 239);
            this.comboBoxCoatingLayer.Name = "comboBoxCoatingLayer";
            this.comboBoxCoatingLayer.Size = new System.Drawing.Size(106, 21);
            this.comboBoxCoatingLayer.TabIndex = 45;
            this.comboBoxCoatingLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingLayer_SelectedIndexChanged);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // labelDimensionStyle
            // 
            this.labelDimensionStyle.AutoSize = true;
            this.labelDimensionStyle.Location = new System.Drawing.Point(9, 329);
            this.labelDimensionStyle.Name = "labelDimensionStyle";
            this.labelDimensionStyle.Size = new System.Drawing.Size(80, 13);
            this.labelDimensionStyle.TabIndex = 50;
            this.labelDimensionStyle.Text = "Dimension style";
            // 
            // labelDimensionLayer
            // 
            this.labelDimensionLayer.AutoSize = true;
            this.labelDimensionLayer.Location = new System.Drawing.Point(9, 356);
            this.labelDimensionLayer.Name = "labelDimensionLayer";
            this.labelDimensionLayer.Size = new System.Drawing.Size(81, 13);
            this.labelDimensionLayer.TabIndex = 53;
            this.labelDimensionLayer.Text = "Dimension layer";
            // 
            // comboBoxDimensionStyle
            // 
            this.comboBoxDimensionStyle.FormattingEnabled = true;
            this.comboBoxDimensionStyle.Location = new System.Drawing.Point(100, 326);
            this.comboBoxDimensionStyle.Name = "comboBoxDimensionStyle";
            this.comboBoxDimensionStyle.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionStyle.TabIndex = 49;
            this.comboBoxDimensionStyle.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionStyle_SelectedIndexChanged);
            // 
            // labelDimensionColor
            // 
            this.labelDimensionColor.AutoSize = true;
            this.labelDimensionColor.Location = new System.Drawing.Point(9, 382);
            this.labelDimensionColor.Name = "labelDimensionColor";
            this.labelDimensionColor.Size = new System.Drawing.Size(82, 13);
            this.labelDimensionColor.TabIndex = 54;
            this.labelDimensionColor.Text = "Dimension color";
            // 
            // comboBoxDimensionLayer
            // 
            this.comboBoxDimensionLayer.FormattingEnabled = true;
            this.comboBoxDimensionLayer.Location = new System.Drawing.Point(100, 353);
            this.comboBoxDimensionLayer.Name = "comboBoxDimensionLayer";
            this.comboBoxDimensionLayer.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionLayer.TabIndex = 51;
            this.comboBoxDimensionLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionLayer_SelectedIndexChanged);
            // 
            // radioButtonDimStandartArcLine
            // 
            this.radioButtonDimStandartArcLine.AutoSize = true;
            this.radioButtonDimStandartArcLine.Location = new System.Drawing.Point(7, 19);
            this.radioButtonDimStandartArcLine.Name = "radioButtonDimStandartArcLine";
            this.radioButtonDimStandartArcLine.Size = new System.Drawing.Size(64, 17);
            this.radioButtonDimStandartArcLine.TabIndex = 55;
            this.radioButtonDimStandartArcLine.TabStop = true;
            this.radioButtonDimStandartArcLine.Text = "Arc-Line";
            this.radioButtonDimStandartArcLine.UseVisualStyleBackColor = true;
            this.radioButtonDimStandartArcLine.CheckedChanged += new System.EventHandler(this.radioButtonDimStandartArcs_CheckedChanged);
            // 
            // radioButtonDimStandartProjections
            // 
            this.radioButtonDimStandartProjections.AutoSize = true;
            this.radioButtonDimStandartProjections.Location = new System.Drawing.Point(111, 19);
            this.radioButtonDimStandartProjections.Name = "radioButtonDimStandartProjections";
            this.radioButtonDimStandartProjections.Size = new System.Drawing.Size(77, 17);
            this.radioButtonDimStandartProjections.TabIndex = 56;
            this.radioButtonDimStandartProjections.TabStop = true;
            this.radioButtonDimStandartProjections.Text = "Projections";
            this.radioButtonDimStandartProjections.UseVisualStyleBackColor = true;
            this.radioButtonDimStandartProjections.CheckedChanged += new System.EventHandler(this.radioButtonDimStandartProjections_CheckedChanged);
            // 
            // comboBoxCoatingColor
            // 
            this.comboBoxCoatingColor.FormattingEnabled = true;
            this.comboBoxCoatingColor.Location = new System.Drawing.Point(100, 266);
            this.comboBoxCoatingColor.Name = "comboBoxCoatingColor";
            this.comboBoxCoatingColor.Size = new System.Drawing.Size(106, 21);
            this.comboBoxCoatingColor.SortAlphabetically = true;
            this.comboBoxCoatingColor.TabIndex = 57;
            this.comboBoxCoatingColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingColor_SelectedIndexChanged);
            // 
            // comboBoxMaterialColor
            // 
            this.comboBoxMaterialColor.FormattingEnabled = true;
            this.comboBoxMaterialColor.Location = new System.Drawing.Point(100, 134);
            this.comboBoxMaterialColor.Name = "comboBoxMaterialColor";
            this.comboBoxMaterialColor.Size = new System.Drawing.Size(106, 21);
            this.comboBoxMaterialColor.SortAlphabetically = true;
            this.comboBoxMaterialColor.TabIndex = 58;
            this.comboBoxMaterialColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxMaterialColor_SelectedIndexChanged);
            // 
            // comboBoxDimensionColor
            // 
            this.comboBoxDimensionColor.FormattingEnabled = true;
            this.comboBoxDimensionColor.Location = new System.Drawing.Point(100, 379);
            this.comboBoxDimensionColor.Name = "comboBoxDimensionColor";
            this.comboBoxDimensionColor.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDimensionColor.SortAlphabetically = true;
            this.comboBoxDimensionColor.TabIndex = 59;
            this.comboBoxDimensionColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxDimensionColor_SelectedIndexChanged);
            // 
            // groupBoxDimensionLinesStandart
            // 
            this.groupBoxDimensionLinesStandart.Controls.Add(this.radioButtonDimStandartArcLine);
            this.groupBoxDimensionLinesStandart.Controls.Add(this.radioButtonDimStandartProjections);
            this.groupBoxDimensionLinesStandart.Location = new System.Drawing.Point(12, 406);
            this.groupBoxDimensionLinesStandart.Name = "groupBoxDimensionLinesStandart";
            this.groupBoxDimensionLinesStandart.Size = new System.Drawing.Size(194, 43);
            this.groupBoxDimensionLinesStandart.TabIndex = 60;
            this.groupBoxDimensionLinesStandart.TabStop = false;
            this.groupBoxDimensionLinesStandart.Text = "Dimension lines standart";
            // 
            // numericUpDownInternalRadius
            // 
            this.numericUpDownInternalRadius.DecimalPlaces = 2;
            this.numericUpDownInternalRadius.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownInternalRadius.Location = new System.Drawing.Point(129, 30);
            this.numericUpDownInternalRadius.Name = "numericUpDownInternalRadius";
            this.numericUpDownInternalRadius.Size = new System.Drawing.Size(77, 20);
            this.numericUpDownInternalRadius.TabIndex = 61;
            this.numericUpDownInternalRadius.ThousandsSeparator = true;
            this.numericUpDownInternalRadius.ValueChanged += new System.EventHandler(this.numericUpDownInternalRadius_ValueChanged);
            // 
            // SheetProfileGenerationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(215, 493);
            this.Controls.Add(this.numericUpDownInternalRadius);
            this.Controls.Add(this.groupBoxDimensionLinesStandart);
            this.Controls.Add(this.comboBoxDimensionColor);
            this.Controls.Add(this.comboBoxMaterialColor);
            this.Controls.Add(this.comboBoxCoatingColor);
            this.Controls.Add(this.comboBoxDimensionLayer);
            this.Controls.Add(this.labelDimensionColor);
            this.Controls.Add(this.labelCoatingColor);
            this.Controls.Add(this.comboBoxDimensionStyle);
            this.Controls.Add(this.labelCoatingLayer);
            this.Controls.Add(this.labelDimensionLayer);
            this.Controls.Add(this.labelDimensionStyle);
            this.Controls.Add(this.comboBoxCoatingLayer);
            this.Controls.Add(this.labelMaterialColor);
            this.Controls.Add(this.labelMaterialLayer);
            this.Controls.Add(this.comboBoxMaterialLayer);
            this.Controls.Add(this.checkBoxDrawCoating);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.numericUpDownThickness);
            this.Controls.Add(this.checkBoxDrawDimensions);
            this.Controls.Add(this.groupBoxCoating);
            this.Controls.Add(this.groupBoxSide);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelInternalRadius);
            this.Controls.Add(this.labelSheetThickness);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SheetProfileGenerationForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Sheet Profile Generator";
            this.Load += new System.EventHandler(this.SheetProfileGenerationForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SheetProfileGenerationForm_Paint);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SheetProfileGenerationForm_KeyPress);
            this.groupBoxSide.ResumeLayout(false);
            this.groupBoxSide.PerformLayout();
            this.groupBoxCoating.ResumeLayout(false);
            this.groupBoxCoating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.groupBoxDimensionLinesStandart.ResumeLayout(false);
            this.groupBoxDimensionLinesStandart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInternalRadius)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Label labelInternalRadius;
    private System.Windows.Forms.Label labelSheetThickness;
    private System.Windows.Forms.RadioButton radioButtonMaterialSideLeft;
    private System.Windows.Forms.GroupBox groupBoxSide;
    private System.Windows.Forms.RadioButton radioButtonMaterialSideCenter;
    private System.Windows.Forms.RadioButton radioButtonMaterialSideRight;
    private System.Windows.Forms.GroupBox groupBoxCoating;
    private System.Windows.Forms.CheckBox checkBoxDrawDimensions;
    private System.Windows.Forms.NumericUpDown numericUpDownThickness;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.RadioButton radioButtonCoatingRight;
    private System.Windows.Forms.RadioButton radioButtonCoatingBoth;
      private System.Windows.Forms.RadioButton radioButtonCoatingLeft;
    private System.Windows.Forms.CheckBox checkBoxDrawCoating;
    private System.Windows.Forms.ComboBox comboBoxMaterialLayer;
    private System.Windows.Forms.Label labelMaterialLayer;
    private System.Windows.Forms.Label labelMaterialColor;
    private System.Windows.Forms.Label labelCoatingColor;
    private System.Windows.Forms.Label labelCoatingLayer;
    private System.Windows.Forms.ComboBox comboBoxCoatingLayer;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.ComboBox comboBoxDimensionLayer;
    private System.Windows.Forms.Label labelDimensionColor;
    private System.Windows.Forms.ComboBox comboBoxDimensionStyle;
    private System.Windows.Forms.Label labelDimensionLayer;
    private System.Windows.Forms.Label labelDimensionStyle;
    private System.Windows.Forms.RadioButton radioButtonDimStandartProjections;
    private System.Windows.Forms.RadioButton radioButtonDimStandartArcLine;
    private colorComboboxControl.ColorComboBox comboBoxCoatingColor;
    private colorComboboxControl.ColorComboBox comboBoxMaterialColor;
    private colorComboboxControl.ColorComboBox comboBoxDimensionColor;
    private System.Windows.Forms.GroupBox groupBoxDimensionLinesStandart;
      private System.Windows.Forms.NumericUpDown numericUpDownInternalRadius;
  }
}