namespace KojtoCAD.GraphicItems.Gear
{
  partial class ImportGearForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportGearForm));
            this.labelGear = new System.Windows.Forms.Label();
            this.labelNumTeeth = new System.Windows.Forms.Label();
            this.buttonDrawGear = new System.Windows.Forms.Button();
            this.buttonDrawRack = new System.Windows.Forms.Button();
            this.numericUpDownGearModule = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownGearTeethCount = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGearModule)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGearTeethCount)).BeginInit();
            this.SuspendLayout();
            // 
            // labelGear
            // 
            this.labelGear.AutoSize = true;
            this.labelGear.Location = new System.Drawing.Point(13, 23);
            this.labelGear.Name = "labelGear";
            this.labelGear.Size = new System.Drawing.Size(85, 13);
            this.labelGear.TabIndex = 0;
            this.labelGear.Text = "Gear module (M)\r\n";
            // 
            // labelNumTeeth
            // 
            this.labelNumTeeth.AutoSize = true;
            this.labelNumTeeth.Location = new System.Drawing.Point(16, 61);
            this.labelNumTeeth.Name = "labelNumTeeth";
            this.labelNumTeeth.Size = new System.Drawing.Size(103, 13);
            this.labelNumTeeth.TabIndex = 1;
            this.labelNumTeeth.Text = "Number of Teeth (Z)";
            // 
            // buttonDrawGear
            // 
            this.buttonDrawGear.Location = new System.Drawing.Point(12, 97);
            this.buttonDrawGear.Name = "buttonDrawGear";
            this.buttonDrawGear.Size = new System.Drawing.Size(132, 35);
            this.buttonDrawGear.TabIndex = 4;
            this.buttonDrawGear.Text = "Draw Gear";
            this.buttonDrawGear.UseVisualStyleBackColor = true;
            this.buttonDrawGear.Click += new System.EventHandler(this.buttonDrawGear_Click);
            // 
            // buttonDrawRack
            // 
            this.buttonDrawRack.Location = new System.Drawing.Point(160, 97);
            this.buttonDrawRack.Name = "buttonDrawRack";
            this.buttonDrawRack.Size = new System.Drawing.Size(120, 35);
            this.buttonDrawRack.TabIndex = 5;
            this.buttonDrawRack.Text = "Draw Rack";
            this.buttonDrawRack.UseVisualStyleBackColor = true;
            this.buttonDrawRack.Click += new System.EventHandler(this.buttonDrawRack_Click);
            // 
            // numericUpDownGearModule
            // 
            this.numericUpDownGearModule.DecimalPlaces = 3;
            this.numericUpDownGearModule.Location = new System.Drawing.Point(160, 23);
            this.numericUpDownGearModule.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownGearModule.Name = "numericUpDownGearModule";
            this.numericUpDownGearModule.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownGearModule.TabIndex = 2;
            this.numericUpDownGearModule.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGearModule.ValueChanged += new System.EventHandler(this.numericUpDownGearModule_ValueChanged);
            // 
            // numericUpDownGearTeethCount
            // 
            this.numericUpDownGearTeethCount.Location = new System.Drawing.Point(160, 61);
            this.numericUpDownGearTeethCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownGearTeethCount.Name = "numericUpDownGearTeethCount";
            this.numericUpDownGearTeethCount.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownGearTeethCount.TabIndex = 3;
            this.numericUpDownGearTeethCount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownGearTeethCount.ValueChanged += new System.EventHandler(this.numericUpDownGearTeethCount_ValueChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(101, 152);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // ImportGearForm
            // 
            this.ClientSize = new System.Drawing.Size(296, 190);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.numericUpDownGearTeethCount);
            this.Controls.Add(this.numericUpDownGearModule);
            this.Controls.Add(this.buttonDrawRack);
            this.Controls.Add(this.buttonDrawGear);
            this.Controls.Add(this.labelNumTeeth);
            this.Controls.Add(this.labelGear);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ImportGearForm";
            this.Text = "Draw Gear or Rack";
            this.Load += new System.EventHandler(this.ImportGearForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ImportGearForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGearModule)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGearTeethCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label labelGear;
      private System.Windows.Forms.Label labelNumTeeth;
      private System.Windows.Forms.Button buttonDrawGear;
      private System.Windows.Forms.Button buttonDrawRack;
      private System.Windows.Forms.NumericUpDown numericUpDownGearModule;
      private System.Windows.Forms.NumericUpDown numericUpDownGearTeethCount;
      private System.Windows.Forms.Button buttonCancel;
  }
}