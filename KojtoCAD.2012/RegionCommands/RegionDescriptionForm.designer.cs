namespace KojtoCAD.RegionCommands
{
  partial class RegionDescriptionForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose ( bool disposing )
    {
      if ( disposing && (components != null) )
      {
        components.Dispose( );
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ( )
    {
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder1 = new XPTable.Models.DataSourceColumnBinder();
            XPTable.Renderers.DragDropRenderer dragDropRenderer1 = new XPTable.Renderers.DragDropRenderer();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionDescriptionForm));
            this.tableRegDescr = new XPTable.Models.Table();
            this.columnModelRegDescr = new XPTable.Models.ColumnModel();
            this.MPropName = new XPTable.Models.TextColumn();
            this.MPropValue = new XPTable.Models.NumberColumn();
            this.MPropUnit = new XPTable.Models.ComboBoxColumn();
            this.MPropGRADE = new XPTable.Models.TextColumn();
            this.MPropDesc = new XPTable.Models.TextColumn();
            this.tableModelRegDescr = new XPTable.Models.TableModel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelUnits = new System.Windows.Forms.Label();
            this.comboUnits = new System.Windows.Forms.ComboBox();
            this.radioKg = new System.Windows.Forms.RadioButton();
            this.radioG = new System.Windows.Forms.RadioButton();
            this.radioT = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkDrawText = new System.Windows.Forms.CheckBox();
            this.checkDrawDim = new System.Windows.Forms.CheckBox();
            this.comboBoxDimensionStyle = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.tableRegDescr)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableRegDescr
            // 
            this.tableRegDescr.BorderColor = System.Drawing.Color.Black;
            this.tableRegDescr.ColumnModel = this.columnModelRegDescr;
            this.tableRegDescr.DataMember = null;
            this.tableRegDescr.DataSourceColumnBinder = dataSourceColumnBinder1;
            dragDropRenderer1.ForeColor = System.Drawing.Color.Red;
            this.tableRegDescr.DragDropRenderer = dragDropRenderer1;
            this.tableRegDescr.Location = new System.Drawing.Point(12, 12);
            this.tableRegDescr.Name = "tableRegDescr";
            this.tableRegDescr.SelectionBackColor = System.Drawing.SystemColors.Control;
            this.tableRegDescr.Size = new System.Drawing.Size(634, 394);
            this.tableRegDescr.TabIndex = 0;
            this.tableRegDescr.TableModel = this.tableModelRegDescr;
            this.tableRegDescr.Text = "tableRegDescr";
            this.tableRegDescr.UnfocusedBorderColor = System.Drawing.Color.Black;
            this.tableRegDescr.UnfocusedSelectionBackColor = System.Drawing.SystemColors.ActiveBorder;
            // 
            // columnModelRegDescr
            // 
            this.columnModelRegDescr.Columns.AddRange(new XPTable.Models.Column[] {
            this.MPropName,
            this.MPropValue,
            this.MPropUnit,
            this.MPropGRADE,
            this.MPropDesc});
            // 
            // MPropName
            // 
            this.MPropName.IsTextTrimmed = false;
            this.MPropName.Text = "Name";
            this.MPropName.Width = 100;
            // 
            // MPropValue
            // 
            this.MPropValue.Format = ".0000";
            this.MPropValue.IsTextTrimmed = false;
            this.MPropValue.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.MPropValue.Text = "Value";
            this.MPropValue.Width = 250;
            // 
            // MPropUnit
            // 
            this.MPropUnit.IsTextTrimmed = false;
            this.MPropUnit.Text = "Unit";
            this.MPropUnit.Width = 100;
            // 
            // MPropGRADE
            // 
            this.MPropGRADE.IsTextTrimmed = false;
            this.MPropGRADE.Text = "Grade";
            this.MPropGRADE.Width = 50;
            // 
            // MPropDesc
            // 
            this.MPropDesc.IsTextTrimmed = false;
            this.MPropDesc.Text = "Description";
            this.MPropDesc.Width = 125;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(572, 452);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(571, 481);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelUnits
            // 
            this.labelUnits.AutoSize = true;
            this.labelUnits.Location = new System.Drawing.Point(13, 419);
            this.labelUnits.Name = "labelUnits";
            this.labelUnits.Size = new System.Drawing.Size(197, 13);
            this.labelUnits.TabIndex = 3;
            this.labelUnits.Text = "The drawing units of the current dile are ";
            // 
            // comboUnits
            // 
            this.comboUnits.FormattingEnabled = true;
            this.comboUnits.Location = new System.Drawing.Point(215, 413);
            this.comboUnits.Name = "comboUnits";
            this.comboUnits.Size = new System.Drawing.Size(121, 21);
            this.comboUnits.TabIndex = 4;
            // 
            // radioKg
            // 
            this.radioKg.AutoSize = true;
            this.radioKg.Location = new System.Drawing.Point(11, 21);
            this.radioKg.Name = "radioKg";
            this.radioKg.Size = new System.Drawing.Size(103, 17);
            this.radioKg.TabIndex = 5;
            this.radioKg.TabStop = true;
            this.radioKg.Text = "kg / cubic meter";
            this.radioKg.UseVisualStyleBackColor = true;
            // 
            // radioG
            // 
            this.radioG.AutoSize = true;
            this.radioG.Location = new System.Drawing.Point(120, 21);
            this.radioG.Name = "radioG";
            this.radioG.Size = new System.Drawing.Size(100, 17);
            this.radioG.TabIndex = 6;
            this.radioG.TabStop = true;
            this.radioG.Text = "g / cubic  meter";
            this.radioG.UseVisualStyleBackColor = true;
            this.radioG.CheckedChanged += new System.EventHandler(this.radioG_CheckedChanged);
            // 
            // radioT
            // 
            this.radioT.AutoSize = true;
            this.radioT.Location = new System.Drawing.Point(226, 21);
            this.radioT.Name = "radioT";
            this.radioT.Size = new System.Drawing.Size(94, 17);
            this.radioT.TabIndex = 7;
            this.radioT.TabStop = true;
            this.radioT.Text = "t / cubic meter";
            this.radioT.UseVisualStyleBackColor = true;
            this.radioT.CheckedChanged += new System.EventHandler(this.radioT_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioKg);
            this.groupBox1.Controls.Add(this.radioT);
            this.groupBox1.Controls.Add(this.radioG);
            this.groupBox1.Location = new System.Drawing.Point(12, 452);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(324, 52);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Density Units";
            // 
            // checkDrawText
            // 
            this.checkDrawText.AutoSize = true;
            this.checkDrawText.Location = new System.Drawing.Point(364, 417);
            this.checkDrawText.Name = "checkDrawText";
            this.checkDrawText.Size = new System.Drawing.Size(172, 17);
            this.checkDrawText.TabIndex = 9;
            this.checkDrawText.Text = "Create text with the profile data";
            this.checkDrawText.UseVisualStyleBackColor = true;
            // 
            // checkDrawDim
            // 
            this.checkDrawDim.AutoSize = true;
            this.checkDrawDim.Location = new System.Drawing.Point(364, 440);
            this.checkDrawDim.Name = "checkDrawDim";
            this.checkDrawDim.Size = new System.Drawing.Size(173, 17);
            this.checkDrawDim.TabIndex = 10;
            this.checkDrawDim.Text = "Create dimensions of the profile";
            this.checkDrawDim.UseVisualStyleBackColor = true;
            // 
            // comboBoxDimensionStyle
            // 
            this.comboBoxDimensionStyle.FormattingEnabled = true;
            this.comboBoxDimensionStyle.Location = new System.Drawing.Point(364, 462);
            this.comboBoxDimensionStyle.Name = "comboBoxDimensionStyle";
            this.comboBoxDimensionStyle.Size = new System.Drawing.Size(121, 21);
            this.comboBoxDimensionStyle.TabIndex = 11;
            // 
            // RegionDescriptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(654, 516);
            this.Controls.Add(this.comboBoxDimensionStyle);
            this.Controls.Add(this.checkDrawDim);
            this.Controls.Add(this.checkDrawText);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.comboUnits);
            this.Controls.Add(this.labelUnits);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.tableRegDescr);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "RegionDescriptionForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "  Cross section properties";
            this.Load += new System.EventHandler(this.RegionDescriptionForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RegionDescriptionForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.tableRegDescr)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private XPTable.Models.Table tableRegDescr;
    private XPTable.Models.ColumnModel columnModelRegDescr;
    private XPTable.Models.TextColumn MPropName;
    private XPTable.Models.TextColumn MPropDesc;
    private XPTable.Models.TextColumn MPropGRADE;
    private XPTable.Models.NumberColumn MPropValue;
    private XPTable.Models.ComboBoxColumn MPropUnit;
    private XPTable.Models.TableModel tableModelRegDescr;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Label labelUnits;
    private System.Windows.Forms.ComboBox comboUnits;
    private System.Windows.Forms.RadioButton radioKg;
    private System.Windows.Forms.RadioButton radioG;
    private System.Windows.Forms.RadioButton radioT;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox checkDrawText;
    private System.Windows.Forms.CheckBox checkDrawDim;
    private System.Windows.Forms.ComboBox comboBoxDimensionStyle;
  }
}