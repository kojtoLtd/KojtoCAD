namespace KojtoCAD.BlockItems.GlassUnits.Single
{
  partial class ImportSingleGlassUnitForm
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
            this.groupBoxGlass = new System.Windows.Forms.GroupBox();
            this.checkBoxGlassFoil = new System.Windows.Forms.CheckBox();
            this.numericUpDownOuterGlassThickness = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPVBThickness = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownInnerGlassThickness = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownGlassThickness = new System.Windows.Forms.NumericUpDown();
            this.labelInnerLaminatedD2 = new System.Windows.Forms.Label();
            this.labelInnerLaminatedD2MM = new System.Windows.Forms.Label();
            this.labelInnerLaminatedPVB = new System.Windows.Forms.Label();
            this.labelInnerLaminatedPVBLayers = new System.Windows.Forms.Label();
            this.labelInnerLaminatedD = new System.Windows.Forms.Label();
            this.labelInnerLaminatedDMM = new System.Windows.Forms.Label();
            this.labelInnerMonoliticD = new System.Windows.Forms.Label();
            this.labelInnerMonoliticDMM = new System.Windows.Forms.Label();
            this.radioButtonLaminated = new System.Windows.Forms.RadioButton();
            this.radioButtonMonolitic = new System.Windows.Forms.RadioButton();
            this.labelTotalThickness = new System.Windows.Forms.Label();
            this.numericUpDownProfileLenght = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDrawGlassUnit = new System.Windows.Forms.Button();
            this.labelLenghtOfProfileMM = new System.Windows.Forms.Label();
            this.labelLenghtOfProfile = new System.Windows.Forms.Label();
            this.groupBoxGlass.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOuterGlassThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPVBThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInnerGlassThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGlassThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProfileLenght)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxGlass
            // 
            this.groupBoxGlass.Controls.Add(this.checkBoxGlassFoil);
            this.groupBoxGlass.Controls.Add(this.numericUpDownOuterGlassThickness);
            this.groupBoxGlass.Controls.Add(this.numericUpDownPVBThickness);
            this.groupBoxGlass.Controls.Add(this.numericUpDownInnerGlassThickness);
            this.groupBoxGlass.Controls.Add(this.numericUpDownGlassThickness);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedD2);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedD2MM);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedPVB);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedPVBLayers);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedD);
            this.groupBoxGlass.Controls.Add(this.labelInnerLaminatedDMM);
            this.groupBoxGlass.Controls.Add(this.labelInnerMonoliticD);
            this.groupBoxGlass.Controls.Add(this.labelInnerMonoliticDMM);
            this.groupBoxGlass.Controls.Add(this.radioButtonLaminated);
            this.groupBoxGlass.Controls.Add(this.radioButtonMonolitic);
            this.groupBoxGlass.Location = new System.Drawing.Point(12, 12);
            this.groupBoxGlass.Name = "groupBoxGlass";
            this.groupBoxGlass.Size = new System.Drawing.Size(307, 119);
            this.groupBoxGlass.TabIndex = 28;
            this.groupBoxGlass.TabStop = false;
            this.groupBoxGlass.Text = "Glass pane";
            // 
            // checkBoxGlassFoil
            // 
            this.checkBoxGlassFoil.AutoSize = true;
            this.checkBoxGlassFoil.Enabled = false;
            this.checkBoxGlassFoil.Location = new System.Drawing.Point(8, 97);
            this.checkBoxGlassFoil.Name = "checkBoxGlassFoil";
            this.checkBoxGlassFoil.Size = new System.Drawing.Size(93, 17);
            this.checkBoxGlassFoil.TabIndex = 6;
            this.checkBoxGlassFoil.Text = "Inner glass foil";
            this.checkBoxGlassFoil.UseVisualStyleBackColor = true;
            this.checkBoxGlassFoil.Visible = false;
            this.checkBoxGlassFoil.CheckedChanged += new System.EventHandler(this.checkBoxGlassFoil_CheckedChanged);
            // 
            // numericUpDownOuterGlassThickness
            // 
            this.numericUpDownOuterGlassThickness.Location = new System.Drawing.Point(191, 94);
            this.numericUpDownOuterGlassThickness.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownOuterGlassThickness.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownOuterGlassThickness.Name = "numericUpDownOuterGlassThickness";
            this.numericUpDownOuterGlassThickness.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownOuterGlassThickness.TabIndex = 5;
            this.numericUpDownOuterGlassThickness.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOuterGlassThickness.ValueChanged += new System.EventHandler(this.numericUpDownOuterGlassThickness_ValueChanged);
            // 
            // numericUpDownPVBThickness
            // 
            this.numericUpDownPVBThickness.Location = new System.Drawing.Point(191, 68);
            this.numericUpDownPVBThickness.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownPVBThickness.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPVBThickness.Name = "numericUpDownPVBThickness";
            this.numericUpDownPVBThickness.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownPVBThickness.TabIndex = 4;
            this.numericUpDownPVBThickness.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownPVBThickness.ValueChanged += new System.EventHandler(this.numericUpDownPVBThickness_ValueChanged);
            // 
            // numericUpDownInnerGlassThickness
            // 
            this.numericUpDownInnerGlassThickness.Location = new System.Drawing.Point(191, 42);
            this.numericUpDownInnerGlassThickness.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownInnerGlassThickness.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownInnerGlassThickness.Name = "numericUpDownInnerGlassThickness";
            this.numericUpDownInnerGlassThickness.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownInnerGlassThickness.TabIndex = 3;
            this.numericUpDownInnerGlassThickness.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownInnerGlassThickness.ValueChanged += new System.EventHandler(this.numericUpDownInnerGlassThickness_ValueChanged);
            // 
            // numericUpDownGlassThickness
            // 
            this.numericUpDownGlassThickness.Location = new System.Drawing.Point(39, 40);
            this.numericUpDownGlassThickness.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownGlassThickness.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownGlassThickness.Name = "numericUpDownGlassThickness";
            this.numericUpDownGlassThickness.Size = new System.Drawing.Size(42, 20);
            this.numericUpDownGlassThickness.TabIndex = 2;
            this.numericUpDownGlassThickness.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDownGlassThickness.ValueChanged += new System.EventHandler(this.numericUpDownGlassThickness_ValueChanged);
            // 
            // labelInnerLaminatedD2
            // 
            this.labelInnerLaminatedD2.AutoSize = true;
            this.labelInnerLaminatedD2.Location = new System.Drawing.Point(137, 96);
            this.labelInnerLaminatedD2.Name = "labelInnerLaminatedD2";
            this.labelInnerLaminatedD2.Size = new System.Drawing.Size(48, 13);
            this.labelInnerLaminatedD2.TabIndex = 93;
            this.labelInnerLaminatedD2.Text = "d LG 2 =";
            // 
            // labelInnerLaminatedD2MM
            // 
            this.labelInnerLaminatedD2MM.AutoSize = true;
            this.labelInnerLaminatedD2MM.Location = new System.Drawing.Point(247, 96);
            this.labelInnerLaminatedD2MM.Name = "labelInnerLaminatedD2MM";
            this.labelInnerLaminatedD2MM.Size = new System.Drawing.Size(23, 13);
            this.labelInnerLaminatedD2MM.TabIndex = 92;
            this.labelInnerLaminatedD2MM.Text = "mm";
            // 
            // labelInnerLaminatedPVB
            // 
            this.labelInnerLaminatedPVB.AutoSize = true;
            this.labelInnerLaminatedPVB.Location = new System.Drawing.Point(121, 70);
            this.labelInnerLaminatedPVB.Name = "labelInnerLaminatedPVB";
            this.labelInnerLaminatedPVB.Size = new System.Drawing.Size(64, 13);
            this.labelInnerLaminatedPVB.TabIndex = 95;
            this.labelInnerLaminatedPVB.Text = "PVB layers :";
            // 
            // labelInnerLaminatedPVBLayers
            // 
            this.labelInnerLaminatedPVBLayers.AutoSize = true;
            this.labelInnerLaminatedPVBLayers.Location = new System.Drawing.Point(247, 70);
            this.labelInnerLaminatedPVBLayers.Name = "labelInnerLaminatedPVBLayers";
            this.labelInnerLaminatedPVBLayers.Size = new System.Drawing.Size(51, 13);
            this.labelInnerLaminatedPVBLayers.TabIndex = 94;
            this.labelInnerLaminatedPVBLayers.Text = "= xx [mm]";
            // 
            // labelInnerLaminatedD
            // 
            this.labelInnerLaminatedD.AutoSize = true;
            this.labelInnerLaminatedD.Location = new System.Drawing.Point(137, 44);
            this.labelInnerLaminatedD.Name = "labelInnerLaminatedD";
            this.labelInnerLaminatedD.Size = new System.Drawing.Size(48, 13);
            this.labelInnerLaminatedD.TabIndex = 97;
            this.labelInnerLaminatedD.Text = "d LG 1 =";
            // 
            // labelInnerLaminatedDMM
            // 
            this.labelInnerLaminatedDMM.AutoSize = true;
            this.labelInnerLaminatedDMM.Location = new System.Drawing.Point(247, 44);
            this.labelInnerLaminatedDMM.Name = "labelInnerLaminatedDMM";
            this.labelInnerLaminatedDMM.Size = new System.Drawing.Size(23, 13);
            this.labelInnerLaminatedDMM.TabIndex = 96;
            this.labelInnerLaminatedDMM.Text = "mm";
            // 
            // labelInnerMonoliticD
            // 
            this.labelInnerMonoliticD.AutoSize = true;
            this.labelInnerMonoliticD.Location = new System.Drawing.Point(11, 42);
            this.labelInnerMonoliticD.Name = "labelInnerMonoliticD";
            this.labelInnerMonoliticD.Size = new System.Drawing.Size(22, 13);
            this.labelInnerMonoliticD.TabIndex = 99;
            this.labelInnerMonoliticD.Text = "d =";
            // 
            // labelInnerMonoliticDMM
            // 
            this.labelInnerMonoliticDMM.AutoSize = true;
            this.labelInnerMonoliticDMM.Location = new System.Drawing.Point(87, 44);
            this.labelInnerMonoliticDMM.Name = "labelInnerMonoliticDMM";
            this.labelInnerMonoliticDMM.Size = new System.Drawing.Size(23, 13);
            this.labelInnerMonoliticDMM.TabIndex = 98;
            this.labelInnerMonoliticDMM.Text = "mm";
            // 
            // radioButtonLaminated
            // 
            this.radioButtonLaminated.AutoSize = true;
            this.radioButtonLaminated.Location = new System.Drawing.Point(162, 19);
            this.radioButtonLaminated.Name = "radioButtonLaminated";
            this.radioButtonLaminated.Size = new System.Drawing.Size(74, 17);
            this.radioButtonLaminated.TabIndex = 1;
            this.radioButtonLaminated.TabStop = true;
            this.radioButtonLaminated.Text = "Laminated";
            this.radioButtonLaminated.UseVisualStyleBackColor = true;
            this.radioButtonLaminated.CheckedChanged += new System.EventHandler(this.radioButtonLaminated_CheckedChanged);
            // 
            // radioButtonMonolitic
            // 
            this.radioButtonMonolitic.AutoSize = true;
            this.radioButtonMonolitic.Checked = true;
            this.radioButtonMonolitic.Location = new System.Drawing.Point(6, 19);
            this.radioButtonMonolitic.Name = "radioButtonMonolitic";
            this.radioButtonMonolitic.Size = new System.Drawing.Size(67, 17);
            this.radioButtonMonolitic.TabIndex = 0;
            this.radioButtonMonolitic.TabStop = true;
            this.radioButtonMonolitic.Text = "Monolitic";
            this.radioButtonMonolitic.UseVisualStyleBackColor = true;
            this.radioButtonMonolitic.CheckedChanged += new System.EventHandler(this.radioButtonMonolitic_CheckedChanged);
            // 
            // labelTotalThickness
            // 
            this.labelTotalThickness.AutoSize = true;
            this.labelTotalThickness.Location = new System.Drawing.Point(21, 173);
            this.labelTotalThickness.Name = "labelTotalThickness";
            this.labelTotalThickness.Size = new System.Drawing.Size(88, 13);
            this.labelTotalThickness.TabIndex = 98;
            this.labelTotalThickness.Text = "Total thickness =";
            // 
            // numericUpDownProfileLenght
            // 
            this.numericUpDownProfileLenght.Location = new System.Drawing.Point(203, 147);
            this.numericUpDownProfileLenght.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.numericUpDownProfileLenght.Name = "numericUpDownProfileLenght";
            this.numericUpDownProfileLenght.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownProfileLenght.TabIndex = 93;
            this.numericUpDownProfileLenght.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownProfileLenght.ValueChanged += new System.EventHandler(this.numericUpDownProfileLenght_ValueChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(209, 194);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 30);
            this.buttonCancel.TabIndex = 95;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDrawGlassUnit
            // 
            this.buttonDrawGlassUnit.Location = new System.Drawing.Point(18, 194);
            this.buttonDrawGlassUnit.Name = "buttonDrawGlassUnit";
            this.buttonDrawGlassUnit.Size = new System.Drawing.Size(100, 30);
            this.buttonDrawGlassUnit.TabIndex = 94;
            this.buttonDrawGlassUnit.Text = "Draw";
            this.buttonDrawGlassUnit.UseVisualStyleBackColor = true;
            this.buttonDrawGlassUnit.Click += new System.EventHandler(this.buttonDrawGlassUnit_Click);
            // 
            // labelLenghtOfProfileMM
            // 
            this.labelLenghtOfProfileMM.AutoSize = true;
            this.labelLenghtOfProfileMM.Location = new System.Drawing.Point(259, 149);
            this.labelLenghtOfProfileMM.Name = "labelLenghtOfProfileMM";
            this.labelLenghtOfProfileMM.Size = new System.Drawing.Size(23, 13);
            this.labelLenghtOfProfileMM.TabIndex = 96;
            this.labelLenghtOfProfileMM.Text = "mm";
            // 
            // labelLenghtOfProfile
            // 
            this.labelLenghtOfProfile.AutoSize = true;
            this.labelLenghtOfProfile.Location = new System.Drawing.Point(21, 149);
            this.labelLenghtOfProfile.Name = "labelLenghtOfProfile";
            this.labelLenghtOfProfile.Size = new System.Drawing.Size(80, 13);
            this.labelLenghtOfProfile.TabIndex = 97;
            this.labelLenghtOfProfile.Text = "Profile lenght = ";
            // 
            // ImportSingleGlassUnitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 238);
            this.Controls.Add(this.labelTotalThickness);
            this.Controls.Add(this.numericUpDownProfileLenght);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonDrawGlassUnit);
            this.Controls.Add(this.labelLenghtOfProfileMM);
            this.Controls.Add(this.labelLenghtOfProfile);
            this.Controls.Add(this.groupBoxGlass);
            this.KeyPreview = true;
            this.Name = "ImportSingleGlassUnitForm";
            this.Text = "Import Single Glass Unit";
            this.Load += new System.EventHandler(this.ImportSingleGlassUnitForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ImportSingleGlassUnitForm_KeyPress);
            this.groupBoxGlass.ResumeLayout(false);
            this.groupBoxGlass.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOuterGlassThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPVBThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInnerGlassThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGlassThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProfileLenght)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBoxGlass;
    private System.Windows.Forms.NumericUpDown numericUpDownOuterGlassThickness;
    private System.Windows.Forms.NumericUpDown numericUpDownPVBThickness;
    private System.Windows.Forms.NumericUpDown numericUpDownInnerGlassThickness;
    private System.Windows.Forms.NumericUpDown numericUpDownGlassThickness;
    private System.Windows.Forms.Label labelInnerLaminatedD2;
    private System.Windows.Forms.Label labelInnerLaminatedD2MM;
    private System.Windows.Forms.Label labelInnerLaminatedPVB;
    private System.Windows.Forms.Label labelInnerLaminatedPVBLayers;
    private System.Windows.Forms.Label labelInnerLaminatedD;
    private System.Windows.Forms.Label labelInnerLaminatedDMM;
    private System.Windows.Forms.Label labelInnerMonoliticD;
    private System.Windows.Forms.Label labelInnerMonoliticDMM;
    private System.Windows.Forms.RadioButton radioButtonLaminated;
    private System.Windows.Forms.RadioButton radioButtonMonolitic;
    private System.Windows.Forms.Label labelTotalThickness;
    private System.Windows.Forms.NumericUpDown numericUpDownProfileLenght;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonDrawGlassUnit;
    private System.Windows.Forms.Label labelLenghtOfProfileMM;
    private System.Windows.Forms.Label labelLenghtOfProfile;
    private System.Windows.Forms.CheckBox checkBoxGlassFoil;
  }
}