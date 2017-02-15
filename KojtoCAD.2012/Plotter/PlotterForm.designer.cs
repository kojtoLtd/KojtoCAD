namespace KojtoCAD.Plotter
{
  partial class PlotterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlotterForm));
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonPlot = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.checkBoxDWG = new System.Windows.Forms.CheckBox();
            this.cmbPlotStyleTables = new System.Windows.Forms.ComboBox();
            this.checkBoxDXF = new System.Windows.Forms.CheckBox();
            this.cmbPaperSizePrint = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelInputType = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonSourcePath = new System.Windows.Forms.Button();
            this.LabelSource = new System.Windows.Forms.Label();
            this.textBoxSourcePath = new System.Windows.Forms.TextBox();
            this.checkBoxCenterPlot = new System.Windows.Forms.CheckBox();
            this.checkBoxPlotAllDWGs = new System.Windows.Forms.CheckBox();
            this.cmbPlotDevicePrinter = new System.Windows.Forms.ComboBox();
            this.labelOutputType = new System.Windows.Forms.Label();
            this.checkBoxIgnoreModelSpace = new System.Windows.Forms.CheckBox();
            this.checkBoxPDF = new System.Windows.Forms.CheckBox();
            this.checkBoxDWF = new System.Windows.Forms.CheckBox();
            this.checkBoxMultiSheet = new System.Windows.Forms.CheckBox();
            this.checkBoxPrint = new System.Windows.Forms.CheckBox();
            this.cmbPlotDevicePDF = new System.Windows.Forms.ComboBox();
            this.cmbPlotDeviceDWF = new System.Windows.Forms.ComboBox();
            this.cmbPaperSizePDF = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSavePath = new System.Windows.Forms.Button();
            this.cmbPaperSizeDWF = new System.Windows.Forms.ComboBox();
            this.label_savePath_PDF = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxSavePath = new System.Windows.Forms.TextBox();
            this.textBoxDynamicBlockFrameName = new System.Windows.Forms.TextBox();
            this.labelFrameName = new System.Windows.Forms.Label();
            this.labelFrameLayout = new System.Windows.Forms.Label();
            this.textBoxFrameLayout = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxPlotTransparency = new System.Windows.Forms.CheckBox();
            this.checkBoxTurnOnViewPorts = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.labelPrinterPlotter = new System.Windows.Forms.Label();
            this.labelMessageToTheUser = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonSelect_UnselectFiles = new System.Windows.Forms.Button();
            this.LayoutNameList = new System.Windows.Forms.CheckedListBox();
            this.DwgNameList = new System.Windows.Forms.CheckedListBox();
            this.labelDrawings = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BoxDwgLayout = new System.Windows.Forms.GroupBox();
            this.buttonSelect_UnselectLayouts = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.BoxDwgLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonExit.Image = ((System.Drawing.Image)(resources.GetObject("buttonExit.Image")));
            this.buttonExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonExit.Location = new System.Drawing.Point(418, 630);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(85, 22);
            this.buttonExit.TabIndex = 26;
            this.buttonExit.Text = "Cancel";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonPlot
            // 
            this.buttonPlot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPlot.Image = ((System.Drawing.Image)(resources.GetObject("buttonPlot.Image")));
            this.buttonPlot.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonPlot.Location = new System.Drawing.Point(771, 630);
            this.buttonPlot.Name = "buttonPlot";
            this.buttonPlot.Size = new System.Drawing.Size(85, 22);
            this.buttonPlot.TabIndex = 25;
            this.buttonPlot.Text = "Plot";
            this.buttonPlot.UseVisualStyleBackColor = true;
            this.buttonPlot.Click += new System.EventHandler(this.buttonPlot_Click);
            // 
            // checkBoxDWG
            // 
            this.checkBoxDWG.AutoSize = true;
            this.checkBoxDWG.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDWG.Location = new System.Drawing.Point(124, 57);
            this.checkBoxDWG.Name = "checkBoxDWG";
            this.checkBoxDWG.Size = new System.Drawing.Size(56, 17);
            this.checkBoxDWG.TabIndex = 0;
            this.checkBoxDWG.Text = "DWG";
            this.checkBoxDWG.UseVisualStyleBackColor = true;
            this.checkBoxDWG.CheckedChanged += new System.EventHandler(this.checkBoxDWG_CheckedChanged);
            // 
            // cmbPlotStyleTables
            // 
            this.cmbPlotStyleTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlotStyleTables.FormattingEnabled = true;
            this.cmbPlotStyleTables.Location = new System.Drawing.Point(123, 132);
            this.cmbPlotStyleTables.Name = "cmbPlotStyleTables";
            this.cmbPlotStyleTables.Size = new System.Drawing.Size(261, 21);
            this.cmbPlotStyleTables.TabIndex = 19;
            this.cmbPlotStyleTables.SelectedIndexChanged += new System.EventHandler(this.cmbPlotStyleTables_SelectedIndexChanged);
            // 
            // checkBoxDXF
            // 
            this.checkBoxDXF.AutoSize = true;
            this.checkBoxDXF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDXF.Location = new System.Drawing.Point(220, 58);
            this.checkBoxDXF.Name = "checkBoxDXF";
            this.checkBoxDXF.Size = new System.Drawing.Size(50, 17);
            this.checkBoxDXF.TabIndex = 2;
            this.checkBoxDXF.Text = "DXF";
            this.checkBoxDXF.UseVisualStyleBackColor = true;
            this.checkBoxDXF.CheckedChanged += new System.EventHandler(this.checkBoxDXF_CheckedChanged);
            // 
            // cmbPaperSizePrint
            // 
            this.cmbPaperSizePrint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaperSizePrint.FormattingEnabled = true;
            this.cmbPaperSizePrint.Location = new System.Drawing.Point(123, 436);
            this.cmbPaperSizePrint.Name = "cmbPaperSizePrint";
            this.cmbPaperSizePrint.Size = new System.Drawing.Size(261, 21);
            this.cmbPaperSizePrint.TabIndex = 23;
            this.cmbPaperSizePrint.SelectedIndexChanged += new System.EventHandler(this.cmbPaperSize_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Source Plot Style:";
            // 
            // labelInputType
            // 
            this.labelInputType.AutoSize = true;
            this.labelInputType.Location = new System.Drawing.Point(4, 58);
            this.labelInputType.Name = "labelInputType";
            this.labelInputType.Size = new System.Drawing.Size(93, 13);
            this.labelInputType.TabIndex = 3;
            this.labelInputType.Text = "Source File Type :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 442);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Paper size:";
            // 
            // buttonSourcePath
            // 
            this.buttonSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSourcePath.Location = new System.Drawing.Point(351, 82);
            this.buttonSourcePath.Name = "buttonSourcePath";
            this.buttonSourcePath.Size = new System.Drawing.Size(33, 20);
            this.buttonSourcePath.TabIndex = 28;
            this.buttonSourcePath.Text = "...";
            this.buttonSourcePath.UseVisualStyleBackColor = true;
            this.buttonSourcePath.Click += new System.EventHandler(this.buttonSourcePath_Click);
            // 
            // LabelSource
            // 
            this.LabelSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSource.Location = new System.Drawing.Point(4, 88);
            this.LabelSource.Name = "LabelSource";
            this.LabelSource.Size = new System.Drawing.Size(91, 13);
            this.LabelSource.TabIndex = 29;
            this.LabelSource.Text = "Source Folder:";
            // 
            // textBoxSourcePath
            // 
            this.textBoxSourcePath.AllowDrop = true;
            this.textBoxSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSourcePath.Location = new System.Drawing.Point(123, 82);
            this.textBoxSourcePath.Name = "textBoxSourcePath";
            this.textBoxSourcePath.Size = new System.Drawing.Size(219, 20);
            this.textBoxSourcePath.TabIndex = 27;
            this.textBoxSourcePath.TextChanged += new System.EventHandler(this.textBoxSourcePath_TextChanged);
            // 
            // checkBoxCenterPlot
            // 
            this.checkBoxCenterPlot.AutoSize = true;
            this.checkBoxCenterPlot.Location = new System.Drawing.Point(123, 463);
            this.checkBoxCenterPlot.Name = "checkBoxCenterPlot";
            this.checkBoxCenterPlot.Size = new System.Drawing.Size(95, 17);
            this.checkBoxCenterPlot.TabIndex = 0;
            this.checkBoxCenterPlot.Text = "Center the plot";
            this.checkBoxCenterPlot.UseVisualStyleBackColor = true;
            // 
            // checkBoxPlotAllDWGs
            // 
            this.checkBoxPlotAllDWGs.AutoSize = true;
            this.checkBoxPlotAllDWGs.Location = new System.Drawing.Point(123, 108);
            this.checkBoxPlotAllDWGs.Name = "checkBoxPlotAllDWGs";
            this.checkBoxPlotAllDWGs.Size = new System.Drawing.Size(267, 17);
            this.checkBoxPlotAllDWGs.TabIndex = 42;
            this.checkBoxPlotAllDWGs.Text = "Search subdirectories for DWGs in source directory";
            this.checkBoxPlotAllDWGs.UseVisualStyleBackColor = true;
            // 
            // cmbPlotDevicePrinter
            // 
            this.cmbPlotDevicePrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlotDevicePrinter.FormattingEnabled = true;
            this.cmbPlotDevicePrinter.Location = new System.Drawing.Point(123, 407);
            this.cmbPlotDevicePrinter.Name = "cmbPlotDevicePrinter";
            this.cmbPlotDevicePrinter.Size = new System.Drawing.Size(261, 21);
            this.cmbPlotDevicePrinter.TabIndex = 45;
            this.cmbPlotDevicePrinter.SelectedIndexChanged += new System.EventHandler(this.cmbPlotDevice_SelectedIndexChanged);
            // 
            // labelOutputType
            // 
            this.labelOutputType.AutoSize = true;
            this.labelOutputType.Location = new System.Drawing.Point(4, 211);
            this.labelOutputType.Name = "labelOutputType";
            this.labelOutputType.Size = new System.Drawing.Size(88, 13);
            this.labelOutputType.TabIndex = 41;
            this.labelOutputType.Text = "Make PDF Files :";
            // 
            // checkBoxIgnoreModelSpace
            // 
            this.checkBoxIgnoreModelSpace.AutoSize = true;
            this.checkBoxIgnoreModelSpace.Location = new System.Drawing.Point(242, 463);
            this.checkBoxIgnoreModelSpace.Name = "checkBoxIgnoreModelSpace";
            this.checkBoxIgnoreModelSpace.Size = new System.Drawing.Size(152, 17);
            this.checkBoxIgnoreModelSpace.TabIndex = 10;
            this.checkBoxIgnoreModelSpace.Text = "Ignore modelspace layouts";
            this.checkBoxIgnoreModelSpace.UseVisualStyleBackColor = true;
            // 
            // checkBoxPDF
            // 
            this.checkBoxPDF.AutoSize = true;
            this.checkBoxPDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxPDF.Location = new System.Drawing.Point(124, 210);
            this.checkBoxPDF.Name = "checkBoxPDF";
            this.checkBoxPDF.Size = new System.Drawing.Size(50, 17);
            this.checkBoxPDF.TabIndex = 2;
            this.checkBoxPDF.Text = "PDF";
            this.checkBoxPDF.UseVisualStyleBackColor = true;
            // 
            // checkBoxDWF
            // 
            this.checkBoxDWF.AutoSize = true;
            this.checkBoxDWF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDWF.Location = new System.Drawing.Point(124, 291);
            this.checkBoxDWF.Name = "checkBoxDWF";
            this.checkBoxDWF.Size = new System.Drawing.Size(54, 17);
            this.checkBoxDWF.TabIndex = 0;
            this.checkBoxDWF.Text = "DWF";
            this.checkBoxDWF.UseVisualStyleBackColor = true;
            // 
            // checkBoxMultiSheet
            // 
            this.checkBoxMultiSheet.AutoSize = true;
            this.checkBoxMultiSheet.Location = new System.Drawing.Point(123, 509);
            this.checkBoxMultiSheet.Name = "checkBoxMultiSheet";
            this.checkBoxMultiSheet.Size = new System.Drawing.Size(157, 17);
            this.checkBoxMultiSheet.TabIndex = 11;
            this.checkBoxMultiSheet.Text = "Make multisheet PDF/DWF";
            this.checkBoxMultiSheet.UseVisualStyleBackColor = true;
            // 
            // checkBoxPrint
            // 
            this.checkBoxPrint.AutoSize = true;
            this.checkBoxPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxPrint.Location = new System.Drawing.Point(123, 380);
            this.checkBoxPrint.Name = "checkBoxPrint";
            this.checkBoxPrint.Size = new System.Drawing.Size(64, 17);
            this.checkBoxPrint.TabIndex = 46;
            this.checkBoxPrint.Text = "PRINT";
            this.checkBoxPrint.UseVisualStyleBackColor = true;
            // 
            // cmbPlotDevicePDF
            // 
            this.cmbPlotDevicePDF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlotDevicePDF.FormattingEnabled = true;
            this.cmbPlotDevicePDF.Location = new System.Drawing.Point(124, 231);
            this.cmbPlotDevicePDF.Name = "cmbPlotDevicePDF";
            this.cmbPlotDevicePDF.Size = new System.Drawing.Size(260, 21);
            this.cmbPlotDevicePDF.TabIndex = 43;
            this.cmbPlotDevicePDF.SelectedIndexChanged += new System.EventHandler(this.cmbPlotDevicePDF_SelectedIndexChanged);
            // 
            // cmbPlotDeviceDWF
            // 
            this.cmbPlotDeviceDWF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlotDeviceDWF.FormattingEnabled = true;
            this.cmbPlotDeviceDWF.Location = new System.Drawing.Point(123, 315);
            this.cmbPlotDeviceDWF.Name = "cmbPlotDeviceDWF";
            this.cmbPlotDeviceDWF.Size = new System.Drawing.Size(261, 21);
            this.cmbPlotDeviceDWF.TabIndex = 45;
            this.cmbPlotDeviceDWF.SelectedIndexChanged += new System.EventHandler(this.cmbPlotDeviceDWF_SelectedIndexChanged);
            // 
            // cmbPaperSizePDF
            // 
            this.cmbPaperSizePDF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaperSizePDF.FormattingEnabled = true;
            this.cmbPaperSizePDF.Location = new System.Drawing.Point(124, 260);
            this.cmbPaperSizePDF.Name = "cmbPaperSizePDF";
            this.cmbPaperSizePDF.Size = new System.Drawing.Size(260, 21);
            this.cmbPaperSizePDF.TabIndex = 48;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 263);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Paper size:";
            // 
            // buttonSavePath
            // 
            this.buttonSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSavePath.Location = new System.Drawing.Point(351, 181);
            this.buttonSavePath.Name = "buttonSavePath";
            this.buttonSavePath.Size = new System.Drawing.Size(33, 20);
            this.buttonSavePath.TabIndex = 15;
            this.buttonSavePath.Text = "...";
            this.buttonSavePath.UseVisualStyleBackColor = true;
            this.buttonSavePath.Click += new System.EventHandler(this.buttonSavePath_Click);
            // 
            // cmbPaperSizeDWF
            // 
            this.cmbPaperSizeDWF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaperSizeDWF.FormattingEnabled = true;
            this.cmbPaperSizeDWF.Location = new System.Drawing.Point(123, 348);
            this.cmbPaperSizeDWF.Name = "cmbPaperSizeDWF";
            this.cmbPaperSizeDWF.Size = new System.Drawing.Size(261, 21);
            this.cmbPaperSizeDWF.TabIndex = 50;
            // 
            // label_savePath_PDF
            // 
            this.label_savePath_PDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_savePath_PDF.Location = new System.Drawing.Point(4, 184);
            this.label_savePath_PDF.Name = "label_savePath_PDF";
            this.label_savePath_PDF.Size = new System.Drawing.Size(104, 22);
            this.label_savePath_PDF.TabIndex = 16;
            this.label_savePath_PDF.Text = "Destination Folder:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 349);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 49;
            this.label3.Text = "Paper size:";
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSavePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSavePath.Location = new System.Drawing.Point(124, 181);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.Size = new System.Drawing.Size(219, 20);
            this.textBoxSavePath.TabIndex = 14;
            // 
            // textBoxDynamicBlockFrameName
            // 
            this.textBoxDynamicBlockFrameName.Location = new System.Drawing.Point(126, 544);
            this.textBoxDynamicBlockFrameName.Name = "textBoxDynamicBlockFrameName";
            this.textBoxDynamicBlockFrameName.Size = new System.Drawing.Size(258, 20);
            this.textBoxDynamicBlockFrameName.TabIndex = 36;
            // 
            // labelFrameName
            // 
            this.labelFrameName.AutoSize = true;
            this.labelFrameName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelFrameName.Location = new System.Drawing.Point(3, 547);
            this.labelFrameName.Name = "labelFrameName";
            this.labelFrameName.Size = new System.Drawing.Size(117, 13);
            this.labelFrameName.TabIndex = 37;
            this.labelFrameName.Text = "Drawing Frame Names:";
            // 
            // labelFrameLayout
            // 
            this.labelFrameLayout.AutoSize = true;
            this.labelFrameLayout.Location = new System.Drawing.Point(3, 601);
            this.labelFrameLayout.Name = "labelFrameLayout";
            this.labelFrameLayout.Size = new System.Drawing.Size(118, 13);
            this.labelFrameLayout.TabIndex = 51;
            this.labelFrameLayout.Text = "Name of Polyline Layer:";
            // 
            // textBoxFrameLayout
            // 
            this.textBoxFrameLayout.Location = new System.Drawing.Point(127, 598);
            this.textBoxFrameLayout.Name = "textBoxFrameLayout";
            this.textBoxFrameLayout.Size = new System.Drawing.Size(257, 20);
            this.textBoxFrameLayout.TabIndex = 52;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxPlotTransparency);
            this.groupBox2.Controls.Add(this.checkBoxTurnOnViewPorts);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.labelPrinterPlotter);
            this.groupBox2.Controls.Add(this.labelMessageToTheUser);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.textBoxFrameLayout);
            this.groupBox2.Controls.Add(this.labelFrameLayout);
            this.groupBox2.Controls.Add(this.labelFrameName);
            this.groupBox2.Controls.Add(this.textBoxDynamicBlockFrameName);
            this.groupBox2.Controls.Add(this.textBoxSavePath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label_savePath_PDF);
            this.groupBox2.Controls.Add(this.cmbPaperSizeDWF);
            this.groupBox2.Controls.Add(this.buttonSavePath);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cmbPaperSizePDF);
            this.groupBox2.Controls.Add(this.cmbPlotDeviceDWF);
            this.groupBox2.Controls.Add(this.cmbPlotDevicePDF);
            this.groupBox2.Controls.Add(this.checkBoxPrint);
            this.groupBox2.Controls.Add(this.checkBoxMultiSheet);
            this.groupBox2.Controls.Add(this.checkBoxDWF);
            this.groupBox2.Controls.Add(this.checkBoxPDF);
            this.groupBox2.Controls.Add(this.checkBoxIgnoreModelSpace);
            this.groupBox2.Controls.Add(this.labelOutputType);
            this.groupBox2.Controls.Add(this.cmbPlotDevicePrinter);
            this.groupBox2.Controls.Add(this.checkBoxPlotAllDWGs);
            this.groupBox2.Controls.Add(this.checkBoxCenterPlot);
            this.groupBox2.Controls.Add(this.textBoxSourcePath);
            this.groupBox2.Controls.Add(this.LabelSource);
            this.groupBox2.Controls.Add(this.buttonSourcePath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.labelInputType);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cmbPaperSizePrint);
            this.groupBox2.Controls.Add(this.checkBoxDXF);
            this.groupBox2.Controls.Add(this.cmbPlotStyleTables);
            this.groupBox2.Controls.Add(this.checkBoxDWG);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(8, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(400, 638);
            this.groupBox2.TabIndex = 42;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Options";
            // 
            // checkBoxPlotTransparency
            // 
            this.checkBoxPlotTransparency.AutoSize = true;
            this.checkBoxPlotTransparency.Location = new System.Drawing.Point(286, 487);
            this.checkBoxPlotTransparency.Name = "checkBoxPlotTransparency";
            this.checkBoxPlotTransparency.Size = new System.Drawing.Size(112, 17);
            this.checkBoxPlotTransparency.TabIndex = 64;
            this.checkBoxPlotTransparency.Text = "Plot Transparency";
            this.checkBoxPlotTransparency.UseVisualStyleBackColor = true;
            // 
            // checkBoxTurnOnViewPorts
            // 
            this.checkBoxTurnOnViewPorts.AutoSize = true;
            this.checkBoxTurnOnViewPorts.Location = new System.Drawing.Point(123, 486);
            this.checkBoxTurnOnViewPorts.Name = "checkBoxTurnOnViewPorts";
            this.checkBoxTurnOnViewPorts.Size = new System.Drawing.Size(156, 17);
            this.checkBoxTurnOnViewPorts.TabIndex = 63;
            this.checkBoxTurnOnViewPorts.Text = "Turn On disabled Viewports";
            this.checkBoxTurnOnViewPorts.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label12.Location = new System.Drawing.Point(6, 618);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(342, 15);
            this.label12.TabIndex = 62;
            this.label12.Text = "Example:PLOTTI;FrameLayerName1;Layer245;Defpoints";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label11.Location = new System.Drawing.Point(7, 572);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(363, 15);
            this.label11.TabIndex = 61;
            this.label11.Text = "Example: DrawingFrame1;DrawingFrame2;DrawingFrame3";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 412);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(75, 13);
            this.label10.TabIndex = 60;
            this.label10.Text = "Plotter/Printer:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 381);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 13);
            this.label9.TabIndex = 59;
            this.label9.Text = "Print/Plot Files :";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 319);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 13);
            this.label8.TabIndex = 58;
            this.label8.Text = "Plotter/Printer:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 291);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 57;
            this.label7.Text = "Make DWF Files :";
            // 
            // labelPrinterPlotter
            // 
            this.labelPrinterPlotter.AutoSize = true;
            this.labelPrinterPlotter.Location = new System.Drawing.Point(4, 238);
            this.labelPrinterPlotter.Name = "labelPrinterPlotter";
            this.labelPrinterPlotter.Size = new System.Drawing.Size(75, 13);
            this.labelPrinterPlotter.TabIndex = 56;
            this.labelPrinterPlotter.Text = "Plotter/Printer:";
            // 
            // labelMessageToTheUser
            // 
            this.labelMessageToTheUser.AutoSize = true;
            this.labelMessageToTheUser.Font = new System.Drawing.Font("Georgia", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessageToTheUser.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.labelMessageToTheUser.Location = new System.Drawing.Point(68, 25);
            this.labelMessageToTheUser.Name = "labelMessageToTheUser";
            this.labelMessageToTheUser.Size = new System.Drawing.Size(254, 14);
            this.labelMessageToTheUser.TabIndex = 54;
            this.labelMessageToTheUser.Text = "Please Fill The Follwing Form Correctly";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(184, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 53;
            this.label5.Text = "or";
            // 
            // buttonSelect_UnselectFiles
            // 
            this.buttonSelect_UnselectFiles.Location = new System.Drawing.Point(14, 582);
            this.buttonSelect_UnselectFiles.Name = "buttonSelect_UnselectFiles";
            this.buttonSelect_UnselectFiles.Size = new System.Drawing.Size(186, 24);
            this.buttonSelect_UnselectFiles.TabIndex = 14;
            this.buttonSelect_UnselectFiles.Text = "Select/Unselect all files";
            this.buttonSelect_UnselectFiles.UseVisualStyleBackColor = true;
            this.buttonSelect_UnselectFiles.Click += new System.EventHandler(this.buttonSelect_Unselect_Click);
            // 
            // LayoutNameList
            // 
            this.LayoutNameList.FormattingEnabled = true;
            this.LayoutNameList.HorizontalScrollbar = true;
            this.LayoutNameList.Location = new System.Drawing.Point(226, 44);
            this.LayoutNameList.Name = "LayoutNameList";
            this.LayoutNameList.Size = new System.Drawing.Size(200, 529);
            this.LayoutNameList.Sorted = true;
            this.LayoutNameList.TabIndex = 13;
            this.LayoutNameList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.LayoutNames_ItemCheck);
            this.LayoutNameList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LayoutNameList_MouseDown);
            // 
            // DwgNameList
            // 
            this.DwgNameList.FormattingEnabled = true;
            this.DwgNameList.HorizontalScrollbar = true;
            this.DwgNameList.Location = new System.Drawing.Point(11, 44);
            this.DwgNameList.Name = "DwgNameList";
            this.DwgNameList.Size = new System.Drawing.Size(200, 529);
            this.DwgNameList.Sorted = true;
            this.DwgNameList.TabIndex = 12;
            this.DwgNameList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.DwgNameList_ItemCheck);
            this.DwgNameList.SelectedIndexChanged += new System.EventHandler(this.DwgNameList_SelectedIndexChanged);
            this.DwgNameList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DwgNameList_MouseDown);
            // 
            // labelDrawings
            // 
            this.labelDrawings.AutoSize = true;
            this.labelDrawings.Font = new System.Drawing.Font("Georgia", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDrawings.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.labelDrawings.Location = new System.Drawing.Point(11, 25);
            this.labelDrawings.Name = "labelDrawings";
            this.labelDrawings.Size = new System.Drawing.Size(135, 14);
            this.labelDrawings.TabIndex = 14;
            this.labelDrawings.Text = "Drawings Name List";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Georgia", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label6.Location = new System.Drawing.Point(227, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(126, 14);
            this.label6.TabIndex = 15;
            this.label6.Text = "Layouts Name List";
            // 
            // BoxDwgLayout
            // 
            this.BoxDwgLayout.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.BoxDwgLayout.Controls.Add(this.buttonSelect_UnselectLayouts);
            this.BoxDwgLayout.Controls.Add(this.buttonSelect_UnselectFiles);
            this.BoxDwgLayout.Controls.Add(this.label6);
            this.BoxDwgLayout.Controls.Add(this.labelDrawings);
            this.BoxDwgLayout.Controls.Add(this.DwgNameList);
            this.BoxDwgLayout.Controls.Add(this.LayoutNameList);
            this.BoxDwgLayout.Location = new System.Drawing.Point(418, 12);
            this.BoxDwgLayout.Name = "BoxDwgLayout";
            this.BoxDwgLayout.Size = new System.Drawing.Size(438, 612);
            this.BoxDwgLayout.TabIndex = 43;
            this.BoxDwgLayout.TabStop = false;
            this.BoxDwgLayout.Text = "Drawings Options";
            // 
            // buttonSelect_UnselectLayouts
            // 
            this.buttonSelect_UnselectLayouts.Location = new System.Drawing.Point(230, 582);
            this.buttonSelect_UnselectLayouts.Name = "buttonSelect_UnselectLayouts";
            this.buttonSelect_UnselectLayouts.Size = new System.Drawing.Size(186, 24);
            this.buttonSelect_UnselectLayouts.TabIndex = 16;
            this.buttonSelect_UnselectLayouts.Text = "Select/Unselect All layouts";
            this.buttonSelect_UnselectLayouts.UseVisualStyleBackColor = true;
            this.buttonSelect_UnselectLayouts.Click += new System.EventHandler(this.buttonSelect_UnselectLayouts_Click);
            // 
            // PlotterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 662);
            this.Controls.Add(this.BoxDwgLayout);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonPlot);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1200, 900);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(872, 39);
            this.Name = "PlotterForm";
            this.Text = "KojtoCAD Plotter";
            this.Load += new System.EventHandler(this.PlotterForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PlotterForm_KeyPress);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.BoxDwgLayout.ResumeLayout(false);
            this.BoxDwgLayout.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button buttonExit;
    private System.Windows.Forms.Button buttonPlot;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.CheckBox checkBoxDWG;
    private System.Windows.Forms.ComboBox cmbPlotStyleTables;
    private System.Windows.Forms.CheckBox checkBoxDXF;
    private System.Windows.Forms.ComboBox cmbPaperSizePrint;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelInputType;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonSourcePath;
    private System.Windows.Forms.Label LabelSource;
    private System.Windows.Forms.TextBox textBoxSourcePath;
    private System.Windows.Forms.CheckBox checkBoxCenterPlot;
    private System.Windows.Forms.CheckBox checkBoxPlotAllDWGs;
    private System.Windows.Forms.ComboBox cmbPlotDevicePrinter;
    private System.Windows.Forms.Label labelOutputType;
    private System.Windows.Forms.CheckBox checkBoxIgnoreModelSpace;
    private System.Windows.Forms.CheckBox checkBoxPDF;
    private System.Windows.Forms.CheckBox checkBoxDWF;
    private System.Windows.Forms.CheckBox checkBoxMultiSheet;
    private System.Windows.Forms.CheckBox checkBoxPrint;
    private System.Windows.Forms.ComboBox cmbPlotDevicePDF;
    private System.Windows.Forms.ComboBox cmbPlotDeviceDWF;
    private System.Windows.Forms.ComboBox cmbPaperSizePDF;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonSavePath;
    private System.Windows.Forms.ComboBox cmbPaperSizeDWF;
    private System.Windows.Forms.Label label_savePath_PDF;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBoxSavePath;
    private System.Windows.Forms.TextBox textBoxDynamicBlockFrameName;
    private System.Windows.Forms.Label labelFrameName;
    private System.Windows.Forms.Label labelFrameLayout;
    private System.Windows.Forms.TextBox textBoxFrameLayout;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelMessageToTheUser;
    private System.Windows.Forms.Label labelPrinterPlotter;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Button buttonSelect_UnselectFiles;
    private System.Windows.Forms.CheckedListBox LayoutNameList;
    private System.Windows.Forms.CheckedListBox DwgNameList;
    private System.Windows.Forms.Label labelDrawings;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.GroupBox BoxDwgLayout;
    private System.Windows.Forms.Button buttonSelect_UnselectLayouts;
    private System.Windows.Forms.CheckBox checkBoxTurnOnViewPorts;
        private System.Windows.Forms.CheckBox checkBoxPlotTransparency;
    }
}