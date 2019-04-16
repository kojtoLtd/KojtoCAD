using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.PlottingServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#else
using Application = Bricscad.ApplicationServices.Application;
using Bricscad.PlottingServices;
using Exception = Teigha.Runtime.Exception;
#endif

namespace KojtoCAD.Plotter
{
    public partial class PlotterForm : Form
    {
        #region fields and properties

        // current layout publish info
        private PublishInfo _curPubInfo = null;

        // unchecked layout publish info
        private List<PublishInfo> _removed = new List<PublishInfo>();

        private bool _publish = false;

        public bool Publish
        {
            set
            {
                _publish = value;
            }
            get
            {
                return _publish;
            }
        }

        public string SavePath
        {
            get
            {
                return textBoxSavePath.Text;
            }
        }

        public string SourcePath
        {
            get
            {
                return textBoxSavePath.Text;
            }
        }

        public bool WillPlotDWF
        {
            get
            {
                return checkBoxDWF.Checked;
            }
        }

        public bool WillPlotPDF
        {
            get
            {
                return checkBoxPDF.Checked;
            }
        }

        public bool WillReadDWG
        {
            get
            {
                return checkBoxDWG.Checked;
            }
        }

        public bool WillReadDXF
        {
            get
            {
                return checkBoxDXF.Checked;
            }
        }

        public bool PlotAllDWGsInSource
        {
            get
            {
                return checkBoxPlotAllDWGs.Checked;
            }
        }

        public bool PlotMultiSheet
        {
            get
            {
                return checkBoxMultiSheet.Checked;
            }
        }

        public bool Print
        {
            get
            {
                return checkBoxPrint.Checked;
            }
        }

        public string PlotStyleTable
        {
            get
            {
                return cmbPlotStyleTables.Text;
            }
        }

        public string DevicePlot
        {
            get
            {
                return cmbPlotDevicePrinter.Text;
            }
        }

        public string DevicePDF
        {
            get
            {
                return cmbPlotDevicePDF.Text;
            }
        }

        public string DeviceDWF
        {
            get
            {
                return cmbPlotDeviceDWF.Text;
            }
        }

        public string PaperSize
        {
            get
            {
                return cmbPaperSizePrint.Text;
            }
        }

        public bool IgnoreModelSpace
        {
            get
            {
                return checkBoxIgnoreModelSpace.Checked;
            }
        }

        public static Regex CanonicalMediaNamesFilter = new Regex(@"\d+\.\d+");

        private bool _authorizedCheckDwg;

        private bool _authorizedCheckLayout;

        private bool _checkedChangedWithoutAuthorizationDwg;

        private bool _checkedChangedWithoutAuthorizationLayout;

        private bool _layoutWasChackedManualy;

        #endregion

        #region constructors and onLoad

        public PlotterForm()
        {
            InitializeComponent();
        }

        private void PlotterForm_Load(object sender, EventArgs e)
        {
            #region load default settings

            try
            {
                cmbPlotDevicePrinter.Text = Properties.Settings.Default.plotDevicePrinter;
                cmbPlotDeviceDWF.Text = Properties.Settings.Default.plotDeviceDwf;
                cmbPlotDevicePDF.Text = Properties.Settings.Default.plotDevicePdf;

                cmbPaperSizePrint.Text = Properties.Settings.Default.plotPaperSizePrint;
                cmbPaperSizePDF.Text = Properties.Settings.Default.plotPaperSizePdf;
                cmbPaperSizeDWF.Text = Properties.Settings.Default.plotPaperSizeDwf;

                cmbPlotStyleTables.Text = Properties.Settings.Default.plotStyleTable;

                checkBoxDWG.Checked = Properties.Settings.Default.plotReadDwg;
                checkBoxDXF.Checked = Properties.Settings.Default.plotReadDxf;

                checkBoxPrint.Checked = Properties.Settings.Default.plotMakePrint;
                checkBoxDWF.Checked = Properties.Settings.Default.plotMakeDwf;
                checkBoxPDF.Checked = Properties.Settings.Default.plotMakePdf;

                checkBoxPlotAllDWGs.Checked = Properties.Settings.Default.plotPlotAllDWGs;
                checkBoxIgnoreModelSpace.Checked = Properties.Settings.Default.plotIgnoreModelSpace;
                checkBoxMultiSheet.Checked = Properties.Settings.Default.plotMultiSheet;

                textBoxSourcePath.Text = Properties.Settings.Default.plotSourcePath;
                textBoxSavePath.Text = Properties.Settings.Default.plotSavePath;

                checkBoxTurnOnViewPorts.Checked = Properties.Settings.Default.plotTurnOnViewPorts;
                checkBoxCenterPlot.Checked = Properties.Settings.Default.plotCenterPlot;

                textBoxDynamicBlockFrameName.Text = Properties.Settings.Default.plotDrawingFrameName;
                textBoxFrameLayout.Text = Properties.Settings.Default.plotDrawingFrameLayer;

                Properties.Settings.Default.Save();

                if (!Directory.Exists(textBoxSourcePath.Text))
                {
                    textBoxSourcePath.Text = "Select source directory.";
                }

                if (!Directory.Exists(textBoxSavePath.Text))
                {
                    textBoxSavePath.Text = "Select destination directory.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source);
                //buttonPlot.Enabled = false;
            }
            buttonPlot.Enabled = true;

            #endregion

            #region load devices, paper sizes

            LoadPlotDevicePrinter();
            LoadPlotDevicePDF();
            LoadPlotDeviceDWF();
            LoadPlotStylesTables();

            #endregion
        }

        #endregion

        #region checkboxes actions

        private void checkBoxDWG_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxDWG.Checked)
            {
                foreach (PublishInfo info in KojtoCAD.Plotter.Plotter.PubInfos)
                {
                    if (Same(Path.GetExtension(info.DwgName), ".DWG"))
                    {
                        // Make sure not to add if its skipped
                        if (_removed.Contains(info))
                        {
                            _removed.Remove(info);
                        }
                        this._checkedChangedWithoutAuthorizationDwg = true;
                        DwgNameList.Items.Add(info, !info.SkipDwg);
                        this._checkedChangedWithoutAuthorizationDwg = false;
                    }
                }
            }
            else
            {
                foreach (object obj in DwgNameList.Items)
                {
                    PublishInfo info = (PublishInfo)obj;
                    if (Same(Path.GetExtension(info.DwgName), ".DWG"))
                    {
                        _removed.Add(info);
                    }
                }

                foreach (PublishInfo info in _removed)
                {
                    DwgNameList.Items.Remove(info);
                }
            }

            if (DwgNameList.Items.Count > 0)
            {
                DwgNameList.SelectedIndex = 0;
            }
        }

        private void checkBoxDXF_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxDXF.Checked)
            {
                foreach (PublishInfo info in KojtoCAD.Plotter.Plotter.PubInfos)
                {
                    if (Same(Path.GetExtension(info.DwgName), ".DXF"))
                    {
                        if (_removed.Contains(info))
                        {
                            _removed.Remove(info);
                        }
                        this._checkedChangedWithoutAuthorizationDwg = true;
                        DwgNameList.Items.Add(info, !info.SkipDwg);
                        this._checkedChangedWithoutAuthorizationDwg = false;
                    }
                }
            }
            else
            {
                foreach (object obj in DwgNameList.Items)
                {
                    PublishInfo info = (PublishInfo)obj;
                    if (Same(Path.GetExtension(info.DwgName), ".DXF"))
                    {
                        _removed.Add(info);
                    }
                }
                foreach (PublishInfo info in _removed)
                {
                    DwgNameList.Items.Remove(info);
                }
            }

            if (DwgNameList.Items.Count > 0)
            {
                DwgNameList.SelectedIndex = 0;
            }
        }

        #endregion

        #region textBox actions

        private void textBoxSourcePath_TextChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBoxSourcePath.Text))
            {
                DwgNameList.ClearSelected();
                LayoutNameList.ClearSelected();
                textBoxSourcePath.Focus();
                return;
            }

            if (
                !KojtoCAD.Plotter.Plotter.BuildList(
                    textBoxSourcePath.Text, checkBoxDWG.Checked, checkBoxDXF.Checked, checkBoxPlotAllDWGs.Checked))
            {
                //throw new Exception("The plotter failed to build the drawing list. Maybe you have to select different source directory.");
                return;
            }
            // Refreshing the list boxes the ugly way
            if (this.checkBoxDWG.Checked)
            {
                this.checkBoxDWG.Checked = false;
                this.checkBoxDWG.Checked = true;
            }
            if (this.checkBoxDXF.Checked)
            {
                this.checkBoxDXF.Checked = false;
                this.checkBoxDXF.Checked = true;
            }
        }

        #endregion

        #region comboBox action

        private void cmbPlotDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.plotDevicePrinter = cmbPlotDevicePrinter.Text;
            UpdatePaperListbox(cmbPlotDevicePrinter, cmbPaperSizePrint, cmbPlotDevicePrinter.Text);
        }

        private void cmbPlotDevicePDF_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.plotDevicePdf = cmbPlotDevicePDF.Text;
            UpdatePaperListbox(cmbPlotDevicePDF, cmbPaperSizePDF, cmbPlotDevicePDF.Text);
        }

        private void cmbPlotDeviceDWF_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.plotDeviceDwf = cmbPlotDeviceDWF.Text;
            UpdatePaperListbox(cmbPlotDeviceDWF, cmbPaperSizeDWF, cmbPlotDeviceDWF.Text);
        }

        public void UpdatePaperListbox(ComboBox aCmbPlotDevice, ComboBox aCmbPaperSize, string aDeviceName)
        {
            aCmbPaperSize.Items.Clear();

            if (aCmbPlotDevice.Text == "None" || string.IsNullOrEmpty(aCmbPlotDevice.Text))
            {
                return;
            }

            try
            {
                PlotConfig pc = PlotConfigManager.SetCurrentConfig(aDeviceName);
                if (pc.IsPlotToFile || pc.PlotToFileCapability == PlotToFileCapability.MustPlotToFile || pc.PlotToFileCapability == PlotToFileCapability.PlotToFileAllowed)
                {
                    aCmbPaperSize.Items.Add("Auto");
                }

                foreach (string str in pc.CanonicalMediaNames)
                {
                    if (CanonicalMediaNamesFilter.IsMatch(str))
                    {
#if !bcad
                        aCmbPaperSize.Items.Add(pc.GetLocalMediaName(str));
#else
                        aCmbPaperSize.Items.Add(str);
#endif
                    }
                }
                aCmbPaperSize.Text = aCmbPaperSize.Items[0].ToString();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
            }
        }

        public void LoadPlotDevicePrinter()
        {
            bool deviceSelected = false;

            // Plot devices
            var deviceNames = new UtilityClass().PlotDevicesNames();
            foreach (var deviceName in deviceNames)
            {
                cmbPlotDevicePrinter.Items.Add(deviceName);
                if (string.Compare(deviceName, Properties.Settings.Default.plotDevicePrinter) == 0)
                {
                    deviceSelected = true;
                }
            }

            if (deviceSelected)
            {
                cmbPlotDevicePrinter.Text = Properties.Settings.Default.plotDevicePrinter;
                UpdatePaperListbox(
                    cmbPlotDevicePrinter, cmbPaperSizePrint, Properties.Settings.Default.plotDevicePrinter);
            }
            else
            {
                cmbPlotDevicePrinter.Text = cmbPlotDevicePrinter.Items[0].ToString();
                UpdatePaperListbox(cmbPlotDevicePrinter, cmbPaperSizePrint, cmbPlotDevicePrinter.Items[0].ToString());
            }

            cmbPaperSizePrint.Items.Add("Auto");
        }

        public void LoadPlotDevicePDF()
        {
            bool deviceSelected = false;

            // Plot devices
            var deviceNames = new UtilityClass().PlotDevicesNames();
            foreach (var deviceName in deviceNames)
            {

                cmbPlotDevicePDF.Items.Add(deviceName);
                if (string.Compare(deviceName, Properties.Settings.Default.plotDevicePdf) == 0)
                {
                    deviceSelected = true;
                }
            }

            if (deviceSelected)
            {
                cmbPlotDevicePDF.Text = Properties.Settings.Default.plotDevicePdf;
                UpdatePaperListbox(cmbPlotDevicePDF, cmbPaperSizePDF, Properties.Settings.Default.plotDevicePdf);
            }
            else
            {
                cmbPlotDevicePDF.Text = cmbPlotDevicePDF.Items[0].ToString();
                UpdatePaperListbox(cmbPlotDevicePDF, cmbPaperSizePDF, cmbPlotDevicePDF.Items[0].ToString());
            }
        }

        public void LoadPlotDeviceDWF()
        {
            bool deviceSelected = false;

            // Plot devices
            var deviceNames = new UtilityClass().PlotDevicesNames();
            foreach (var deviceName in deviceNames)
            {

                cmbPlotDevicePDF.Items.Add(deviceName);
                if (string.Compare(deviceName, Properties.Settings.Default.plotDevicePdf) == 0)
                {
                    deviceSelected = true;
                }
            }

            if (deviceSelected)
            {
                cmbPlotDeviceDWF.Text = Properties.Settings.Default.plotDeviceDwf;
                UpdatePaperListbox(cmbPlotDeviceDWF, cmbPaperSizeDWF, Properties.Settings.Default.plotDeviceDwf);
            }
            else
            {
                cmbPlotDeviceDWF.Text = cmbPlotDeviceDWF.Items[0].ToString();
                UpdatePaperListbox(cmbPlotDeviceDWF, cmbPaperSizeDWF, cmbPlotDeviceDWF.Items[0].ToString());
            }
        }

        public void LoadPlotStylesTables()
        {
            bool deviceSelected = false;

            // Plot style tables
            var ctbNames = new UtilityClass().PltoStylesNames();
            foreach (string str in ctbNames)
            {
                string[] tempStrArray = str.Split('\\');
                cmbPlotStyleTables.Items.Add(tempStrArray[tempStrArray.Length - 1]);

                if (string.Compare(tempStrArray[tempStrArray.Length - 1], Properties.Settings.Default.plotStyleTable)
                    == 0)
                {
                    deviceSelected = true;
                }
            }

            cmbPlotStyleTables.Text = deviceSelected ? Properties.Settings.Default.plotStyleTable : cmbPlotStyleTables.Items[0].ToString();
        }

        #endregion

        #region buttons actions

        private void buttonSelect_Unselect_Click(object sender, EventArgs e)
        {
            bool allAreChecked = DwgNameList.CheckedItems.Count == DwgNameList.Items.Count;
            int count = DwgNameList.Items.Count;
            for (int i = 0; i < count; i++)
            {
                PublishInfo info = (PublishInfo)DwgNameList.Items[i];
                _checkedChangedWithoutAuthorizationDwg = true;
                DwgNameList.SetItemChecked(DwgNameList.Items.IndexOf(info), !allAreChecked);
                _checkedChangedWithoutAuthorizationDwg = false;
            }

            if (_curPubInfo != null)
            {
                int nCount = LayoutNameList.Items.Count;
                for (int i = 0; i < nCount; i++)
                {
                    LayoutInfo info = (LayoutInfo)LayoutNameList.Items[i];
                    _checkedChangedWithoutAuthorizationLayout = true;
                    LayoutNameList.SetItemChecked(LayoutNameList.Items.IndexOf(info), !allAreChecked);
                    _checkedChangedWithoutAuthorizationLayout = false;
                }
            }
        }

        private void buttonSelect_UnselectLayouts_Click(object sender, EventArgs e)
        {
            bool allAreChecked = LayoutNameList.CheckedItems.Count == LayoutNameList.Items.Count;
            int nCount = LayoutNameList.Items.Count;
            for (int i = 0; i < nCount; i++)
            {
                LayoutInfo info = (LayoutInfo)LayoutNameList.Items[i];
                _checkedChangedWithoutAuthorizationLayout = true;
                LayoutNameList.SetItemChecked(LayoutNameList.Items.IndexOf(info), !allAreChecked);
                _checkedChangedWithoutAuthorizationLayout = false;
            }
        }

        private void buttonPlot_Click(object sender, EventArgs e)
        {

            #region check textBoxSourcePath

            // Check for all the inputs...
            if (!Directory.Exists(this.textBoxSourcePath.Text))
            {
                // Show error message and return...
                Application.ShowAlertDialog("Specify the DWG/DXF source directory.");
                this.textBoxSourcePath.Focus();
                return;
            }

            #endregion

            #region check textBoxSavePath

            // Setting the focus based on the check box selection (if not set)
            if (checkBoxDWF.Checked || checkBoxPDF.Checked)
            {
                if (textBoxSavePath.Text.Length == 0)
                {
                    // Show error message and return...
                    Application.ShowAlertDialog(
                        "Please specify destination directory.");
                    textBoxSavePath.Focus();
                    return;
                }
                if (!Directory.Exists(this.textBoxSavePath.Text))
                {
                    // Show error message and return...
                    Application.ShowAlertDialog(
                         "Specified directory " + this.textBoxSavePath.Text + " not present.");
                    textBoxSavePath.Focus();
                    return;
                }

            }

            #endregion

            #region check PDF device

            if (checkBoxPDF.Checked)
            {
                if (cmbPlotDevicePDF.Text.Length < 4)
                {
                    cmbPlotDevicePDF.Text = "Select PDF device!";
                    cmbPlotDevicePDF.Focus();
                }
            }

            #endregion

            #region check DWF device

            if (checkBoxPDF.Checked)
            {
                if (cmbPlotDevicePDF.Text.Length < 4)
                {
                    cmbPlotDeviceDWF.Text = "Select DWF device!";
                    cmbPlotDeviceDWF.Focus();
                }
            }

            #endregion

            if (textBoxDynamicBlockFrameName.Text.Length < 1)
            {
                MessageBox.Show(
                    "Provide dynamic block frame name.",
                    "No dynamic block frame name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            if (checkBoxPrint.Checked)
            {
                if (
                    MessageBox.Show(
                        "Are you shure you want to REALLY PRINT all these DWGs ? If otherwise click NO, unckeck 'Print' option and try again.",
                        "Real print confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            if (checkBoxMultiSheet.Checked
                && (cmbPaperSizePDF.Text.Equals("Auto", StringComparison.CurrentCultureIgnoreCase)
                    || cmbPaperSizeDWF.Text.Equals("Auto", StringComparison.CurrentCultureIgnoreCase)))
            {
                DialogResult warning =
                    MessageBox.Show(
                        "Multi sheet PDF with different frame format is not allowed. If your drawings have different frames press Cancel and separate them by frame.",
                        "KojtoCADPlotter",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);
                if (warning == DialogResult.Cancel)
                {
                    cmbPlotDevicePDF.Focus();
                    return;
                }

                //return;
            }

            _publish = true;
            this.DialogResult = DialogResult.OK;

            // Save the current inputs to the Winforms properties
            SaveSettings();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Publish = false;
            Close();
        }

        private void buttonSourcePath_Click(object sender, EventArgs e)
        {
            // Changed from FolderBrowserDialog to OpenFileDialog, because
            // if you manage to click on drive letter C: for example (by mistake) and
            // you have chosen to 'plot all drawings in the source directory'
            // the program starts traversing the whole drive and freezes!
            // To avoid this we ask the user to chose a file 
            // and then we get only the current drawing folder and traverse it.

            /*
   //folderBrowserDialog = new FolderBrowserDialog();
   openFileDialog = new OpenFileDialog();
     
   if (openFileDialog.ShowDialog() == DialogResult.OK)
   {
     textBoxSourcePath.Text = openFileDialog.FileName.Remove(openFileDialog.FileName.LastIndexOf('\\'));
   }

   if (!Directory.Exists(textBoxSourcePath.Text))
   {
     // Show error message and return...
     textBoxSourcePath.Focus();
     return;               
   }
             * */
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Properties.Settings.Default.plotSourcePath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (folderBrowserDialog.SelectedPath.Length == 3)
                {
                    textBoxSourcePath.Text = "You cannot select a whole drive.";
                    textBoxSourcePath.Focus();
                    return;
                }
                textBoxSourcePath.Text = folderBrowserDialog.SelectedPath;
                //textBoxSavePath.Text = folderBrowserDialog.SelectedPath;
                textBoxSavePath.Text = textBoxSourcePath.Text;
            }

        }

        private void buttonSavePath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSavePath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void cmbPlotStyleTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.plotStyleTable = cmbPlotStyleTables.Text;
        }

        private void cmbPaperSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.plotPaperSizePrint = cmbPaperSizePrint.Text;
        }

        #endregion

        #region checkList actions

        private void DwgNameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LayoutNameList.Items.Clear();

            if (DwgNameList.SelectedIndex < 0)
            {
                _curPubInfo = null;
                return;
            }

            _curPubInfo = (PublishInfo)DwgNameList.Items[DwgNameList.SelectedIndex];

            foreach (LayoutInfo layoutInfo in _curPubInfo.LayoutInfos)
            {
                _checkedChangedWithoutAuthorizationLayout = true;
                LayoutNameList.Items.Add(layoutInfo, layoutInfo.Publish);
                _checkedChangedWithoutAuthorizationLayout = false;
            }
        }

        private void DwgNameList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }
            if (!this._checkedChangedWithoutAuthorizationDwg)
            {
                if (!this._authorizedCheckDwg)
                {
                    e.NewValue = e.CurrentValue; //check state change was not through authorized actions
                    return;
                }
            }
            PublishInfo info = (PublishInfo)DwgNameList.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                info.SkipDwg = false;

                if (info != null)
                {
                    foreach (LayoutInfo layoutInfo in info.LayoutInfos)
                    {
                        if (!_layoutWasChackedManualy)
                        {
                            layoutInfo.Publish = true;
                        }
                    }
                }

                //LayoutNameList.Enabled = true;
            }
            else
            {
                info.SkipDwg = true;

                //update the layouts....
                if (info != null)
                {
                    foreach (LayoutInfo layoutInfo in info.LayoutInfos)
                    {
                        layoutInfo.Publish = false;
                    }
                }
                //LayoutNameList.Enabled = false;
            }
        }

        private void LayoutNames_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            if (this._curPubInfo == null)
            {
                return;
            }
            if (!_checkedChangedWithoutAuthorizationLayout)
            {
                if (!this._authorizedCheckLayout)
                {
                    e.NewValue = e.CurrentValue; //check state change was not through authorized actions
                    return;
                }
            }
            var layoutInfo = (LayoutInfo)this.LayoutNameList.Items[e.Index];

            layoutInfo.Publish = e.NewValue == CheckState.Checked;
            var checkedItemsCount = this.LayoutNameList.CheckedItems.Count + (e.NewValue == CheckState.Checked ? 1 : -1);
            if (checkedItemsCount < LayoutNameList.Items.Count)
            {
                _layoutWasChackedManualy = true;
            }
            this._checkedChangedWithoutAuthorizationDwg = true;
            this.DwgNameList.SetItemChecked(this.DwgNameList.SelectedIndex, checkedItemsCount != 0);
            this._checkedChangedWithoutAuthorizationDwg = false;
            _layoutWasChackedManualy = false;
        }

        #endregion

        #region settings functions

        public void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.plotDevicePrinter = cmbPlotDevicePrinter.Text;
                Properties.Settings.Default.plotDevicePdf = cmbPlotDevicePDF.Text;
                Properties.Settings.Default.plotDeviceDwf = cmbPlotDeviceDWF.Text;

                Properties.Settings.Default.plotPaperSizePrint = cmbPaperSizePrint.Text;
                Properties.Settings.Default.plotPaperSizePdf = cmbPaperSizePDF.Text;
                Properties.Settings.Default.plotPaperSizeDwf = cmbPaperSizeDWF.Text;

                Properties.Settings.Default.plotStyleTable = cmbPlotStyleTables.Text;

                Properties.Settings.Default.plotReadDwg = checkBoxDWG.Checked;
                Properties.Settings.Default.plotReadDxf = checkBoxDXF.Checked;

                Properties.Settings.Default.plotMakePrint = checkBoxPrint.Checked;
                Properties.Settings.Default.plotMakeDwf = checkBoxDWF.Checked;
                Properties.Settings.Default.plotMakePdf = checkBoxPDF.Checked;

                Properties.Settings.Default.plotSourcePath = textBoxSourcePath.Text;
                Properties.Settings.Default.plotSavePath = textBoxSavePath.Text;

                Properties.Settings.Default.plotPlotAllDWGs = checkBoxPlotAllDWGs.Checked;
                Properties.Settings.Default.plotIgnoreModelSpace = checkBoxIgnoreModelSpace.Checked;
                Properties.Settings.Default.plotMultiSheet = checkBoxMultiSheet.Checked;
                Properties.Settings.Default.plotTurnOnViewPorts = checkBoxTurnOnViewPorts.Checked;
                Properties.Settings.Default.plotCenterPlot = checkBoxCenterPlot.Checked;

                Properties.Settings.Default.plotDrawingFrameName = textBoxDynamicBlockFrameName.Text;
                Properties.Settings.Default.plotDrawingFrameLayer = textBoxFrameLayout.Text;

                Properties.Settings.Default.plotTransparency = checkBoxPlotTransparency.Checked;

                Properties.Settings.Default.Save();
            }
            catch
            {
                throw new System.Exception("Failed to save plot settings.");
            }
        }

        #endregion

        #region string functions

        internal static bool Same(string first, string second)
        {
            return (string.Compare(first, second, true) == 0);
        }

        #endregion

        #region KeyPressFunctions

        private void PlotterForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }

        private void DwgNameList_MouseDown(object sender, MouseEventArgs e)
        {
            var cursorPosition = new Point(e.X, e.Y);
            for (int i = 0; i < this.DwgNameList.Items.Count; i++)
            {
                Rectangle rec = this.DwgNameList.GetItemRectangle(i);
                rec.Width = 16; //checkbox itself has a default width of about 16 pixels

                if (!rec.Contains(cursorPosition))
                {
                    continue;
                }
                this._authorizedCheckDwg = true;
                bool newValue = !this.DwgNameList.GetItemChecked(i);
                this.DwgNameList.SetItemChecked(i, newValue); //check 
                this._authorizedCheckDwg = false;

                return;
            }
        }

        private void LayoutNameList_MouseDown(object sender, MouseEventArgs e)
        {
            var cursorPosition = new Point(e.X, e.Y);
            for (int i = 0; i < this.LayoutNameList.Items.Count; i++)
            {
                Rectangle rec = this.LayoutNameList.GetItemRectangle(i);
                rec.Width = 16; //checkbox itself has a default width of about 16 pixels

                if (!rec.Contains(cursorPosition))
                {
                    continue;
                }
                this._authorizedCheckLayout = true;
                bool newValue = !this.LayoutNameList.GetItemChecked(i);
                this.LayoutNameList.SetItemChecked(i, newValue); //check 
                this._authorizedCheckLayout = false;

                return;
            }
        }

        #endregion
    }
}