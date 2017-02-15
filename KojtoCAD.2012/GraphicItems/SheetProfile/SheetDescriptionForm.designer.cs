namespace KojtoCAD.GraphicItems.SheetProfile
{
  partial class SheetDescriptionForm
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
            this.dataGridViewMaterials = new System.Windows.Forms.DataGridView();
            this.textBoxThickness = new System.Windows.Forms.TextBox();
            this.labelThickness = new System.Windows.Forms.Label();
            this.comboBoxCoatingColor = new colorComboboxControl.ColorComboBox();
            this.labelCoatingColor = new System.Windows.Forms.Label();
            this.labelCoatingLayer = new System.Windows.Forms.Label();
            this.comboBoxCoatingLayer = new System.Windows.Forms.ComboBox();
            this.checkBoxDrawCoating = new System.Windows.Forms.CheckBox();
            this.groupBoxCoating = new System.Windows.Forms.GroupBox();
            this.radioButtonCoatingRight = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingBoth = new System.Windows.Forms.RadioButton();
            this.radioButtonCoatingLeft = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterials)).BeginInit();
            this.groupBoxCoating.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewMaterials
            // 
            this.dataGridViewMaterials.AllowUserToOrderColumns = true;
            this.dataGridViewMaterials.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewMaterials.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewMaterials.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMaterials.Location = new System.Drawing.Point(0, 12);
            this.dataGridViewMaterials.Name = "dataGridViewMaterials";
            this.dataGridViewMaterials.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewMaterials.Size = new System.Drawing.Size(248, 157);
            this.dataGridViewMaterials.TabIndex = 0;
            this.dataGridViewMaterials.SelectionChanged += new System.EventHandler(this.dataGridViewMaterials_SelectionChanged);
            // 
            // textBoxThickness
            // 
            this.textBoxThickness.Enabled = false;
            this.textBoxThickness.Location = new System.Drawing.Point(127, 191);
            this.textBoxThickness.Name = "textBoxThickness";
            this.textBoxThickness.Size = new System.Drawing.Size(100, 20);
            this.textBoxThickness.TabIndex = 1;
            // 
            // labelThickness
            // 
            this.labelThickness.AutoSize = true;
            this.labelThickness.Location = new System.Drawing.Point(13, 194);
            this.labelThickness.Name = "labelThickness";
            this.labelThickness.Size = new System.Drawing.Size(83, 13);
            this.labelThickness.TabIndex = 2;
            this.labelThickness.Text = "Sheet thickness";
            // 
            // comboBoxCoatingColor
            // 
            this.comboBoxCoatingColor.FormattingEnabled = true;
            this.comboBoxCoatingColor.Location = new System.Drawing.Point(118, 315);
            this.comboBoxCoatingColor.Name = "comboBoxCoatingColor";
            this.comboBoxCoatingColor.Size = new System.Drawing.Size(109, 21);
            this.comboBoxCoatingColor.SortAlphabetically = true;
            this.comboBoxCoatingColor.TabIndex = 63;
            this.comboBoxCoatingColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingColor_SelectedIndexChanged);
            // 
            // labelCoatingColor
            // 
            this.labelCoatingColor.AutoSize = true;
            this.labelCoatingColor.Location = new System.Drawing.Point(9, 318);
            this.labelCoatingColor.Name = "labelCoatingColor";
            this.labelCoatingColor.Size = new System.Drawing.Size(88, 13);
            this.labelCoatingColor.TabIndex = 62;
            this.labelCoatingColor.Text = "Coating line color";
            // 
            // labelCoatingLayer
            // 
            this.labelCoatingLayer.AutoSize = true;
            this.labelCoatingLayer.Location = new System.Drawing.Point(9, 291);
            this.labelCoatingLayer.Name = "labelCoatingLayer";
            this.labelCoatingLayer.Size = new System.Drawing.Size(87, 13);
            this.labelCoatingLayer.TabIndex = 61;
            this.labelCoatingLayer.Text = "Coating line layer";
            // 
            // comboBoxCoatingLayer
            // 
            this.comboBoxCoatingLayer.FormattingEnabled = true;
            this.comboBoxCoatingLayer.Location = new System.Drawing.Point(118, 291);
            this.comboBoxCoatingLayer.Name = "comboBoxCoatingLayer";
            this.comboBoxCoatingLayer.Size = new System.Drawing.Size(109, 21);
            this.comboBoxCoatingLayer.TabIndex = 60;
            this.comboBoxCoatingLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxCoatingLayer_SelectedIndexChanged);
            // 
            // checkBoxDrawCoating
            // 
            this.checkBoxDrawCoating.AutoSize = true;
            this.checkBoxDrawCoating.Location = new System.Drawing.Point(12, 217);
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
            this.groupBoxCoating.Location = new System.Drawing.Point(12, 238);
            this.groupBoxCoating.Name = "groupBoxCoating";
            this.groupBoxCoating.Size = new System.Drawing.Size(215, 46);
            this.groupBoxCoating.TabIndex = 58;
            this.groupBoxCoating.TabStop = false;
            this.groupBoxCoating.Text = "Location";
            // 
            // radioButtonCoatingRight
            // 
            this.radioButtonCoatingRight.AutoSize = true;
            this.radioButtonCoatingRight.Location = new System.Drawing.Point(156, 19);
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
            this.radioButtonCoatingBoth.Location = new System.Drawing.Point(80, 19);
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
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(134, 354);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 30);
            this.buttonCancel.TabIndex = 65;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(7, 354);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 30);
            this.buttonOK.TabIndex = 64;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // SheetDescriptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 393);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxCoatingColor);
            this.Controls.Add(this.labelCoatingColor);
            this.Controls.Add(this.labelCoatingLayer);
            this.Controls.Add(this.comboBoxCoatingLayer);
            this.Controls.Add(this.checkBoxDrawCoating);
            this.Controls.Add(this.groupBoxCoating);
            this.Controls.Add(this.labelThickness);
            this.Controls.Add(this.textBoxThickness);
            this.Controls.Add(this.dataGridViewMaterials);
            this.KeyPreview = true;
            this.Name = "SheetDescriptionForm";
            this.Text = "Sheet Description";
            this.Load += new System.EventHandler(this.SheetDescriptionForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SheetDescriptionForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterials)).EndInit();
            this.groupBoxCoating.ResumeLayout(false);
            this.groupBoxCoating.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView dataGridViewMaterials;
    private System.Windows.Forms.TextBox textBoxThickness;
    private System.Windows.Forms.Label labelThickness;
    private colorComboboxControl.ColorComboBox comboBoxCoatingColor;
    private System.Windows.Forms.Label labelCoatingColor;
    private System.Windows.Forms.Label labelCoatingLayer;
    private System.Windows.Forms.ComboBox comboBoxCoatingLayer;
    private System.Windows.Forms.CheckBox checkBoxDrawCoating;
    private System.Windows.Forms.GroupBox groupBoxCoating;
    private System.Windows.Forms.RadioButton radioButtonCoatingRight;
    private System.Windows.Forms.RadioButton radioButtonCoatingBoth;
    private System.Windows.Forms.RadioButton radioButtonCoatingLeft;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
  }
}