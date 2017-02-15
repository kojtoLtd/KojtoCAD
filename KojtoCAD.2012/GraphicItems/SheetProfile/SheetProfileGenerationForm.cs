using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Application = Bricscad.ApplicationServices.Application;
#endif
namespace KojtoCAD.GraphicItems.SheetProfile
{
    public delegate void SHP_delegateHandler();
    public partial class SheetProfileGenerationForm : Form
    {


        public event SHP_delegateHandler PrepareRedraw;
        public SheetProfile.SheetProfileConfig SpConfig;

        private ObjectId _polylineId;
        private SheetProfile.CoatingSide _coatingSide;
        private SheetProfile.OffsetSide _offsetSide;
        private SheetProfile.DimensionsStandart _dimensionsStandart;

        private bool _formIsInitialized;
        private bool _formHasError;
        private readonly DocumentHelper _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);


        public SheetProfileGenerationForm(ObjectId aPolylineId)
        {
            _polylineId = aPolylineId;
            InitializeComponent();
        }

        private void SheetProfileGenerationForm_Load(object sender, EventArgs e)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

            numericUpDownThickness.Value = Settings.Default.SPG_Thickness;

            numericUpDownInternalRadius.Value = Settings.Default.SPG_InternalRadius;
            checkBoxDrawDimensions.Checked = Settings.Default.SPG_HasDimensions;

            // material side
            if (Settings.Default.SPG_MaterialSide == 1)  //MaterialSide.Left
            {
                radioButtonMaterialSideLeft.Checked = true;
            }
            else if (Settings.Default.SPG_MaterialSide == 2) //MaterialSide.Right
            {
                radioButtonMaterialSideRight.Checked = true;
            }
            else if (Settings.Default.SPG_MaterialSide == 3) //MaterialSide.Center
            {
                radioButtonMaterialSideCenter.Checked = true;
            }

            // coating side
            if (Settings.Default.SPG_CoatingSide == 1) // CoatingSide.Left
            {
                radioButtonCoatingLeft.Checked = true;
            }
            else if (Settings.Default.SPG_CoatingSide == 2) // CoatingSide.Right
            {
                radioButtonCoatingRight.Checked = true;
            }
            else if (Settings.Default.SPG_CoatingSide == 3) // CoatingSide.Both
            {
                radioButtonCoatingBoth.Checked = true;
            }


            if (Settings.Default.SPG_HasCoating)
            {
                checkBoxDrawCoating.Checked = true;
                groupBoxCoating.Enabled = true;
            }
            else
            {
                checkBoxDrawCoating.Checked = false;
                groupBoxCoating.Enabled = false;
                comboBoxCoatingColor.Enabled = false;
                comboBoxCoatingLayer.Enabled = false;
            }

            // Load layers 
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(db.LayerTableId, OpenMode.ForRead);

                comboBoxCoatingLayer.Sorted = true;
                comboBoxDimensionLayer.Sorted = true;
                comboBoxMaterialLayer.Sorted = true;

                comboBoxCoatingLayer.Items.Add("Current Layer");
                comboBoxDimensionLayer.Items.Add("Current Layer");
                comboBoxMaterialLayer.Items.Add("Current Layer");

                //currentIndex = 0;
                foreach (var layerId in layerTable)
                {
                    var layerRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);

                    // coating combo
                    comboBoxCoatingLayer.Items.Add(layerRecord.Name);

                    if (layerRecord.Name == Settings.Default.SPG_CoatingLayer)
                    {
                        comboBoxCoatingLayer.SelectedText = layerRecord.Name;
                    }

                    // dimensions combo
                    comboBoxDimensionLayer.Items.Add(layerRecord.Name);
                    if (layerRecord.Name == Settings.Default.SPG_DimensionLayer)
                    {
                        comboBoxDimensionLayer.SelectedText = layerRecord.Name;
                    }

                    // material combo
                    comboBoxMaterialLayer.Items.Add(layerRecord.Name);
                    if (layerRecord.Name == Settings.Default.SPG_MaterialLayer)
                    {
                        comboBoxMaterialLayer.SelectedText = layerRecord.Name;
                    }
                    //currentIndex++;
                }

                transaction.Commit();
            }

            // Load Dimension Styles
            var dimStyles = drawingHelper.GetDimensionStylesList();

            var currentIndex = 0;
            foreach (var dimStyle in dimStyles)
            {
                comboBoxDimensionStyle.Items.Add(dimStyle);
                if (Settings.Default.SPG_DimensionStyle == dimStyle)
                {
                    comboBoxDimensionStyle.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }

            if (!Settings.Default.SPG_HasDimensions)
            {
                comboBoxDimensionColor.Enabled = false;
                comboBoxDimensionLayer.Enabled = false;
                comboBoxDimensionStyle.Enabled = false;
            }
            // Load Colors

            var color = Color.FromName("ByLayer");
            comboBoxCoatingColor.Items.Insert(0, color);
            comboBoxMaterialColor.Items.Insert(0, color);
            comboBoxDimensionColor.Items.Insert(0, color);



            for (var i = 0; i < comboBoxMaterialColor.Items.Count; i++)
            {
                if ((Color)comboBoxMaterialColor.Items[i] == Settings.Default.SPG_MaterialColor)
                {
                    comboBoxMaterialColor.SelectedIndex = i;
                }
            }

            for (var i = 0; i < comboBoxCoatingColor.Items.Count; i++)
            {
                if ((Color)comboBoxCoatingColor.Items[i] == Settings.Default.SPG_CoatingColor)
                {
                    comboBoxCoatingColor.SelectedIndex = i;
                }
            }

            for (var i = 0; i < comboBoxDimensionColor.Items.Count; i++)
            {
                if ((Color)comboBoxDimensionColor.Items[i] == Settings.Default.SPG_DimensionColor)
                {
                    comboBoxDimensionColor.SelectedIndex = i;
                }
            }

            // Load Dimension Standart
            if (Settings.Default.SPG_DimensionStandart == "ArcsAndLines")
            {
                radioButtonDimStandartArcLine.Checked = true;
                radioButtonDimStandartProjections.Checked = false;
            }

            if (Settings.Default.SPG_DimensionStandart == "Projections")
            {
                radioButtonDimStandartArcLine.Checked = false;
                radioButtonDimStandartProjections.Checked = true;
            }

            _formIsInitialized = true;
            RedrawProfile();
        }

        private void SheetProfileGenerationForm_Paint(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            var gfx = e.Graphics;
            // Create a new pen that we shall use for drawing the line
            var myPen = new Pen(Color.Black);

            gfx.DrawLine(myPen, 1, 160, 220, 160);
            gfx.DrawLine(myPen, 1, 292, 220, 292);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            SheetProfile.ClearEventA();
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                DialogResult = DialogResult.OK;
                SheetProfile.ClearEvent();
            }
            catch
            {
                DialogResult = DialogResult.OK;
                SaveSettings();
            }
        }

        private void comboBoxMaterialLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.SheetLayer = comboBoxMaterialLayer.Text;
            RedrawProfile();
        }

        private void comboBoxDimensionLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.DimensionsLayer = comboBoxDimensionLayer.Text;
            RedrawProfile();
        }

        private void comboBoxCoatingLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.CoatingLayer = comboBoxCoatingLayer.Text;
            RedrawProfile();
        }

        private void comboBoxDimensionColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.DimensionsColor = (Color)comboBoxDimensionColor.SelectedItem;
            RedrawProfile();
        }

        private void comboBoxCoatingColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.CoatingColor = (Color)comboBoxCoatingColor.SelectedItem;
            RedrawProfile();
        }

        private void comboBoxMaterialColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.SheetColor = (Color)comboBoxMaterialColor.SelectedItem;
            RedrawProfile();
        }

        private void comboBoxDimensionStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            SpConfig.DimensionStyle = comboBoxDimensionStyle.Text;
            RedrawProfile();
        }

        private void radioButtonMaterialSideLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMaterialSideLeft.Checked)
            {
                _offsetSide = SheetProfile.OffsetSide.Left;
                RedrawProfile();
            }
        }

        private void radioButtonMaterialSideCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMaterialSideCenter.Checked)
            {
                _offsetSide = SheetProfile.OffsetSide.Center;
                RedrawProfile();
            }
        }

        private void radioButtonMaterialSideRight_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMaterialSideRight.Checked)
            {
                _offsetSide = SheetProfile.OffsetSide.Right;
                RedrawProfile();
            }
        }

        private void radioButtonCoatingLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCoatingLeft.Checked)
            {
                _coatingSide = SheetProfile.CoatingSide.Left;
                RedrawProfile();
            }
        }

        private void radioButtonCoatingRight_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCoatingRight.Checked)
            {
                _coatingSide = SheetProfile.CoatingSide.Right;
                RedrawProfile();
            }
        }

        private void radioButtonCoatingBoth_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCoatingBoth.Checked)
            {
                _coatingSide = SheetProfile.CoatingSide.Both;
                RedrawProfile();
            }
        }

        private void radioButtonDimStandartProjections_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDimStandartProjections.Checked)
            {
                Settings.Default.SPG_DimensionStandart = "Projections";
                _dimensionsStandart = SheetProfile.DimensionsStandart.Projections;
                RedrawProfile();
            }
        }

        private void radioButtonDimStandartArcs_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDimStandartArcLine.Checked)
            {
                Settings.Default.SPG_DimensionStandart = "ArcsAndLines";
                _dimensionsStandart = SheetProfile.DimensionsStandart.ArcsAndLines;
                RedrawProfile();
            }
        }
        private void numericUpDownThickness_ValueChanged(object sender, EventArgs e)
        {
            SpConfig.InternalRadius = (double)numericUpDownThickness.Value;

            if (numericUpDownThickness.Value > 0)
            {
                _formHasError = false;
                RedrawProfile();
            }
            else
            {
                MessageBox.Show("Set a positive thickness and try again.");
                _formHasError = true;
                numericUpDownThickness.Focus();
            }

        }

        private void numericUpDownInternalRadius_ValueChanged(object sender, EventArgs e)
        {
            //mInternalRadius = numericUpDownInternalRadius.Value;
            SpConfig.InternalRadius = (double)numericUpDownInternalRadius.Value;
            if (numericUpDownInternalRadius.Value >= 0)
            {
                _formHasError = false;
                RedrawProfile();
            }
            else
            {
                MessageBox.Show("Set a non-negative radius and try again.");
                _formHasError = true;
                numericUpDownInternalRadius.Focus();
            }
        }

        private void checkBoxDrawCoating_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawCoating.Checked)
            {
                groupBoxCoating.Enabled = true;
                SpConfig.DrawCoating = true;
                comboBoxCoatingColor.Enabled = true;
                comboBoxCoatingLayer.Enabled = true;
            }
            else
            {
                groupBoxCoating.Enabled = false;
                SpConfig.DrawCoating = false;
                comboBoxCoatingColor.Enabled = false;
                comboBoxCoatingLayer.Enabled = false;
            }

            RedrawProfile();
        }

        private void checkBoxDimensions_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawDimensions.Checked)
            {
                comboBoxDimensionColor.Enabled = true;
                comboBoxDimensionLayer.Enabled = true;
                comboBoxDimensionStyle.Enabled = true;
                SpConfig.DrawDimensions = true;
            }
            else
            {
                comboBoxDimensionColor.Enabled = false;
                comboBoxDimensionLayer.Enabled = false;
                comboBoxDimensionStyle.Enabled = false;
                SpConfig.DrawDimensions = false;
            }
            RedrawProfile();
        }

        // validation of thickness
        private void numericUpDownThickness_Validating(object sender, CancelEventArgs e)
        {
            ValidateThickness();
        }
        private void ValidateThickness()
        {
            if (ThicknessIsValid(numericUpDownThickness.Value))
            {
                errorProvider.SetError(numericUpDownThickness, "");
                return;
            }
            errorProvider.SetError(numericUpDownThickness, "Invalid thickness");
            numericUpDownThickness.Focus();
        }
        private bool ThicknessIsValid(decimal aThickness)
        {
            return aThickness > 0;
        }

        // validation of internal radius
        //private void numericUpDownInternalRadius_Validating(object sender, CancelEventArgs e)
        //{
        //    ValidateInternalRadius();
        //}
        //private bool ValidateInternalRadius()
        //{
        //    if (InternalRadiusIsValid(numericUpDownInternalRadius.Value))
        //    {
        //        errorProvider.SetError(numericUpDownInternalRadius, "");
        //        numericUpDownInternalRadius.Focus();
        //        return true;
        //    }
        //    errorProvider.SetError(numericUpDownInternalRadius, "Invalid radius.");
        //    return false;
        //}
        //private bool InternalRadiusIsValid(decimal aInternalRadius)
        //{
        //    return aInternalRadius >= 0;
        //}

        private void SaveSettings()
        {
            Settings.Default.SPG_Thickness = numericUpDownThickness.Value;
            Settings.Default.SPG_InternalRadius = numericUpDownInternalRadius.Value;
            Settings.Default.SPG_HasDimensions = checkBoxDrawDimensions.Checked;

            // material side
            if (radioButtonMaterialSideLeft.Checked)
            {
                Settings.Default.SPG_MaterialSide = 1;
            }
            else if (radioButtonMaterialSideRight.Checked)
            {
                Settings.Default.SPG_MaterialSide = 2;
            }
            else if (radioButtonMaterialSideCenter.Checked)
            {
                Settings.Default.SPG_MaterialSide = 3;
            }

            Settings.Default.SPG_HasCoating = checkBoxDrawCoating.Checked;
            // coating side
            if (radioButtonCoatingLeft.Checked)
            {
                Settings.Default.SPG_CoatingSide = 1;
            }
            else if (radioButtonCoatingRight.Checked)
            {
                Settings.Default.SPG_CoatingSide = 2;
            }
            else if (radioButtonCoatingBoth.Checked)
            {
                Settings.Default.SPG_CoatingSide = 3;
            }

            Settings.Default.SPG_MaterialColor = (Color)comboBoxMaterialColor.SelectedItem;
            Settings.Default.SPG_CoatingColor = (Color)comboBoxCoatingColor.SelectedItem;
            Settings.Default.SPG_DimensionColor = (Color)comboBoxDimensionColor.SelectedItem;

            Settings.Default.SPG_MaterialLayer = comboBoxMaterialLayer.Text;
            Settings.Default.SPG_CoatingLayer = comboBoxCoatingLayer.Text;
            Settings.Default.SPG_DimensionLayer = comboBoxDimensionLayer.Text;
            Settings.Default.SPG_DimensionStyle = comboBoxDimensionStyle.Text;

            if (radioButtonDimStandartArcLine.Checked && !radioButtonDimStandartProjections.Checked)
            {
                Settings.Default.SPG_DimensionStandart = "ArcsAndLines";
            }
            else if (radioButtonDimStandartProjections.Checked && !radioButtonDimStandartArcLine.Checked)
            {
                Settings.Default.SPG_DimensionStandart = "Projections";
            }

            Settings.Default.Save();
        }

        private void RedrawProfile()
        {
            if (_formIsInitialized && !_formHasError)
            {
                PrepareRedraw();

                #region Setup Sheet Config
                SpConfig.PolylineId = _polylineId;

                SpConfig.SheetThickness = (double)numericUpDownThickness.Value;
                SpConfig.InternalRadius = (double)numericUpDownInternalRadius.Value;

                SpConfig.OffsetSide = _offsetSide;
                SpConfig.SheetLayer = comboBoxMaterialLayer.Text;

                // SHEET
                try
                {
                    if (comboBoxMaterialColor.SelectedItem.ToString() == "ByLayer")
                    {
                        SpConfig.SheetColor = _drawingHelper.LayerManipulator.GetLayerColor(comboBoxMaterialLayer.Text);
                    }
                    else
                    {
                        SpConfig.SheetColor = (Color)comboBoxMaterialColor.SelectedItem;
                    }
                }
                catch
                {

                }

                // COATING
                SpConfig.DrawCoating = checkBoxDrawCoating.Checked;
                SpConfig.CoatingSide = _coatingSide;
                SpConfig.CoatingLayer = comboBoxCoatingLayer.Text;
                try
                {
                    if (comboBoxCoatingColor.SelectedItem.ToString() == "ByLayer")
                    {
                        SpConfig.CoatingColor = _drawingHelper.LayerManipulator.GetLayerColor(comboBoxCoatingLayer.Text);
                    }
                    else
                    {
                        SpConfig.CoatingColor = (Color)comboBoxCoatingColor.SelectedItem;
                    }
                }
                catch
                {
                    throw new Exception("Wrong color input.");
                }


                // DIMENSION
                SpConfig.DrawDimensions = checkBoxDrawDimensions.Checked;
                SpConfig.DimensionStyle = comboBoxDimensionStyle.Text;
                SpConfig.DimensionsLayer = comboBoxDimensionLayer.Text;
                try
                {
                    if (comboBoxDimensionColor.SelectedItem.ToString() == "ByLayer")
                    {
                        SpConfig.DimensionsColor = _drawingHelper.LayerManipulator.GetLayerColor(comboBoxDimensionLayer.Text);
                    }
                    else
                    {
                        SpConfig.DimensionsColor = (Color)comboBoxDimensionColor.SelectedItem;
                    }
                }
                catch
                {
                    throw new Exception("Wrong color input.");
                }

                SpConfig.DimensionStandart = _dimensionsStandart;

                #endregion

                SheetProfile.DrawProfile(SpConfig);
            }
        }

        private void SheetProfileGenerationForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }
    }
}