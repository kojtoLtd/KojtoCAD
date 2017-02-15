using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using KojtoCAD.GraphicItems.SheetProfile;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

namespace KojtoCAD.Unfold._2d
{
    public delegate void U2D_delegateHandler();

    public partial class Unfold2dForm : Form
    {

        public event U2D_delegateHandler U2D_PREPARE_REDRAW;
        public UnfoldSheetProfileConfig U2DConfig;

        private bool formIsInitialized;
        private bool formHasError;
        private readonly DocumentHelper _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

        #region Form OnLoad Events and Constructor

        public Unfold2dForm(ObjectId aPolylineId, Point3d aInsertionPoint)
        {
            InitializeComponent();
        }

        public Unfold2dForm(double thickness)
        {
            Settings.Default.U2D_Thickness = (decimal) thickness;
            InitializeComponent();
        }

        private void Unfold2dForm_Load(object sender, EventArgs e)
        {
            DocumentHelper _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

            int currentIndex = 0;

            #region Load thickness

            if ( Settings.Default.U2D_ThicknessIsPredefined )
            {
                numericUpDownThickness.Value = Settings.Default.U2D_Thickness;
            }

            #endregion

            #region Load internal radius

            if ( Settings.Default.U2D_InternalRadiusIsPredefined )
            {
                //numericUpDownInternalRadius.Enabled = true;
                //radioButtonInternalRadiusPredefined.Checked = true;
                //radioButtonInternalRadiusByObject.Checked = false;
                numericUpDownInternalRadius.Value = Settings.Default.U2D_InternalRadius;
            }
            else
            {
                //radioButtonInternalRadiusPredefined.Checked = false;
                //radioButtonInternalRadiusByObject.Checked = true;
                numericUpDownInternalRadius.Value = 0;
                //numericUpDownInternalRadius.Enabled = false;
            }

            #endregion

            #region Load unfolding method

            if ( Settings.Default.U2D_MethodIsBendDeduction ) // && !Settings.Default.U2D_methodIsKFactor
            {
                radioButtonMethodIsBendDeduction.Checked = true;
                textBoxBendDeductionTableFile.Enabled = true;
                if (string.IsNullOrEmpty(Settings.Default.U2D_BendDeductionTableFile) || string.IsNullOrWhiteSpace(Settings.Default.U2D_BendDeductionTableFile))
                {
                    textBoxBendDeductionTableFile.Text = "Select table...";
                }
                else
                {
                    textBoxBendDeductionTableFile.Text = Settings.Default.U2D_BendDeductionTableFile;
                }

                numericUpDownKFactor.Value = 0;
                textBoxKFactorTableFile.Text = "";
                textBoxKFactorTableFile.Enabled = false;

                U2DConfig.UnfoldingMethod = Unfold2D.UnfoldingMethod.BendDeduction;
            }
            else if ( Settings.Default.U2D_MethodIsKFactor )  //&& !Settings.Default.U2D_methodIsBendDeduction
            {
                if (Settings.Default.U2D_KFactor < (decimal)0.001)
                {
                    Settings.Default.U2D_KFactor = (decimal)0.1;
                    numericUpDownKFactor.Value = (decimal) 0.1;
                }

                radioButtonMethodIsKFactor.Checked = true;
                textBoxKFactorTableFile.Enabled = true;
                textBoxKFactorTableFile.Text = Settings.Default.U2D_KFactorTableFile;
                if (string.IsNullOrEmpty(Settings.Default.U2D_KFactorTableFile) || string.IsNullOrWhiteSpace(Settings.Default.U2D_KFactorTableFile) )
                {
                    textBoxKFactorTableFile.Text = "Select table...";
                }
                else
                {
                    textBoxKFactorTableFile.Text = Settings.Default.U2D_BendDeductionTableFile;
                }

                textBoxBendDeductionTableFile.Text = "";
                textBoxKFactorTableFile.Enabled = false;
                U2DConfig.UnfoldingMethod = Unfold2D.UnfoldingMethod.KFactor;

                if (Settings.Default.U2D_KFactorIsPredefined)
                {
                    radioButtonKFactorUniform.Checked = true;
                    numericUpDownKFactor.Value = Settings.Default.U2D_KFactor;
                }
            }

            #endregion

            #region Load Coating Side

            // coating side
            if ( Settings.Default.U2D_CoatingSide == 1 ) // CoatingSide.Left
            {
                radioButtonCoatingLeft.Checked = true;
            }
            else if ( Settings.Default.U2D_CoatingSide == 2 ) // CoatingSide.Right
            {
                radioButtonCoatingRight.Checked = true;
            }
            else if ( Settings.Default.U2D_CoatingSide == 3 ) // CoatingSide.Center
            {
                radioButtonCoatingBoth.Checked = true;
            }


            if ( Settings.Default.U2D_DrawCoating )
            {
                checkBoxDrawCoating.Checked = true;
                groupBoxCoating.Enabled = true;
                U2DConfig.DrawCoating = true;
            }
            else
            {
                checkBoxDrawCoating.Checked = false;
                groupBoxCoating.Enabled = false;
                U2DConfig.DrawCoating = false;
            }
            #endregion

            #region Load Layers
            // Load layers
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;

            using ( Transaction Tr = Db.TransactionManager.StartTransaction() )
            {
                LayerTable LayerTable = (LayerTable) Tr.GetObject(Db.LayerTableId, OpenMode.ForWrite);
                LayerTableRecord LayerRecord;

                comboBoxCoatingLayer.Sorted = true;
                comboBoxDimensionLayer.Sorted = true;

                comboBoxCoatingLayer.Items.Add("Current Layer");
                comboBoxDimensionLayer.Items.Add("Current Layer");

                currentIndex = 0;
                foreach ( ObjectId LayerId in LayerTable )
                {
                    LayerRecord = (LayerTableRecord) Tr.GetObject(LayerId, OpenMode.ForWrite);

                    // coating combo
                    comboBoxCoatingLayer.Items.Add(LayerRecord.Name);
                    if ( LayerRecord.Name == Settings.Default.U2D_CoatingLayer )
                    {
                        comboBoxCoatingLayer.SelectedIndex = currentIndex + 1;
                    }

                    // dimensions combo
                    comboBoxDimensionLayer.Items.Add(LayerRecord.Name);
                    if ( LayerRecord.Name == Settings.Default.U2D_DimensionLayer )
                    {
                        comboBoxDimensionLayer.SelectedIndex = currentIndex + 1;
                    }

                    currentIndex++;
                }
                Tr.Commit();
            }
            #endregion

            #region Load Dimension and Dim Styles
            StringCollection DimStyles = _drawingHelper.GetDimensionStylesList();
            currentIndex = 0;
            comboBoxDimensionStyle.Sorted = true;
            foreach ( string dimStyle in DimStyles )
            {
                comboBoxDimensionStyle.Items.Add(dimStyle);
                if ( Settings.Default.U2D_DimensionStyle == dimStyle )
                {
                    comboBoxDimensionStyle.SelectedIndex = currentIndex;
                }
            }

            if ( Settings.Default.U2D_DrawDimensions )
            {
                checkBoxDrawDimensions.Checked = true;
                comboBoxDimensionStyle.Text = Settings.Default.U2D_DimensionStyle;
            }
            #endregion

            #region Load Colors

            System.Drawing.Color color = new System.Drawing.Color();
            color = System.Drawing.Color.FromName("ByLayer");
            comboBoxCoatingColor.Items.Insert(0, color);
            comboBoxDimensionColor.Items.Insert(0, color);

            for ( int i = 0; i < comboBoxCoatingColor.Items.Count; i++ )
            {
                if ( (System.Drawing.Color) comboBoxCoatingColor.Items[ i ] == Settings.Default.U2D_CoatingColor )
                {
                    comboBoxCoatingColor.SelectedIndex = i;
                }
            }

            for ( int i = 0; i < comboBoxDimensionColor.Items.Count; i++ )
            {
                if ( (System.Drawing.Color) comboBoxDimensionColor.Items[ i ] == Settings.Default.U2D_DimensionColor )
                {
                    comboBoxDimensionColor.SelectedIndex = i;
                }
            }

            #endregion

            /*
      using (Transaction Tr = Db.TransactionManager.StartTransaction())
      {
          System.Drawing.Color color = new Color();
          LayerTable acLyrTbl;
          acLyrTbl = Tr.GetObject(Db.LayerTableId, OpenMode.ForRead) as LayerTable;
          LayerTableRecord acLyrTblRec = Tr.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
          color = System.Drawing.Color.FromName("ByLayer");
          comboBoxCoatingColor.Items.Insert(0, color);
          comboBoxDimensionColor.Items.Insert(0, color);
          color = System.Drawing.Color.FromName("Red");
          comboBoxCoatingColor.Items.Insert(1, color);
          comboBoxDimensionColor.Items.Insert(1, color);
          color = System.Drawing.Color.FromName("Yellow");
          comboBoxCoatingColor.Items.Insert(2, color);
          comboBoxDimensionColor.Items.Insert(2, color);
          color = System.Drawing.Color.FromName("Green");
          comboBoxCoatingColor.Items.Insert(3, color);
          comboBoxDimensionColor.Items.Insert(3, color);
          color = System.Drawing.Color.FromName("Cyan");
          comboBoxCoatingColor.Items.Insert(4, color);
          comboBoxDimensionColor.Items.Insert(4, color);
          color = System.Drawing.Color.FromName("Blue");
          comboBoxCoatingColor.Items.Insert(5, color);
          comboBoxDimensionColor.Items.Insert(5, color);
          color = System.Drawing.Color.FromName("Magenta");
          comboBoxCoatingColor.Items.Insert(6, color);
          comboBoxDimensionColor.Items.Insert(6, color);
          color = System.Drawing.Color.FromName("White");
          comboBoxCoatingColor.Items.Insert(7, color);
          comboBoxDimensionColor.Items.Insert(7, color);
      } */

            formIsInitialized = true;
            Redraw2dUnfolding();
        }
        #endregion

        #region Radiobutton Events
        private void radioButtonInternalRadiusPredefined_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButtonInternalRadiusByObject_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented.");
        }

        private void radioButtonMethodIsBendDeduction_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonMethodIsBendDeduction.Checked )
            {
                buttonSelectBendDeductionTableFile.Enabled = true;
                textBoxBendDeductionTableFile.Enabled = true;

                radioButtonKFactorTable.Enabled = false;
                radioButtonKFactorUniform.Enabled = false;
                buttonSelectKFactorTableFile.Enabled = false;
                //numericUpDownKFactor.Value = 0;
                numericUpDownKFactor.Enabled = false;
                textBoxKFactorTableFile.Text = "";
                textBoxKFactorTableFile.Enabled = false;
                if ( textBoxBendDeductionTableFile.Text != "" )
                {
                    Redraw2dUnfolding();
                }
            }
        }

        private void radioButtonMethodIsKFactor_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonMethodIsKFactor.Checked )
            {
                radioButtonKFactorTable.Enabled = true;
                radioButtonKFactorUniform.Enabled = true;
                numericUpDownKFactor.Enabled = true;
                buttonSelectKFactorTableFile.Enabled = true;
                // textBoxKFactorTableFile.Enabled = true;

                buttonSelectBendDeductionTableFile.Enabled = false;
                textBoxBendDeductionTableFile.Enabled = false;
                //textBoxBendDeductionTableFile.Text = "";
                if ( ( numericUpDownKFactor.Value >= 0 ) && ( numericUpDownKFactor.Value <= 1 ) )
                {
                    Redraw2dUnfolding();
                }
            }
        }

        private void radioButtonKFactorUniform_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonKFactorUniform.Checked )
            {
                U2DConfig.UnfoldingMethod = Unfold2D.UnfoldingMethod.KFactor;
                if ( ( numericUpDownKFactor.Value >= 0 ) && ( numericUpDownKFactor.Value <= 1 ) )
                {
                    U2DConfig.KFactorValue = (double) numericUpDownKFactor.Value;
                    Redraw2dUnfolding();
                }
                else
                {
                    U2DConfig.KFactorValue = -1;
                }
            }
        }

        private void radioButtonKFactorTable_CheckedChanged(object sender, EventArgs e)
        {
            if ( String.IsNullOrEmpty(Settings.Default.U2D_KFactorTableFile) ||
                String.IsNullOrWhiteSpace(Settings.Default.U2D_KFactorTableFile) )
            {
                textBoxKFactorTableFile.Text = "Select table...";
            }
            else
            {
                textBoxKFactorTableFile.Text = Settings.Default.U2D_BendDeductionTableFile;
            }
        }

        private void radioButtonCoatingLeft_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonCoatingLeft.Checked )
            {
                U2DConfig.CoatingSide = SheetProfile.CoatingSide.Left;
                Redraw2dUnfolding();
            }
        }

        private void radioButtonCoatingBoth_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonCoatingBoth.Checked )
            {
                U2DConfig.CoatingSide = SheetProfile.CoatingSide.Both;
                Redraw2dUnfolding();
            }
        }

        private void radioButtonCoatingRight_CheckedChanged(object sender, EventArgs e)
        {
            if ( radioButtonCoatingRight.Checked )
            {
                U2DConfig.CoatingSide = SheetProfile.CoatingSide.Right;
                Redraw2dUnfolding();
            }
        }


        #endregion

        #region Numerical DropDowns Events
        private void numericUpDownThickness_ValueChanged(object sender, EventArgs e)
        {
            if ( numericUpDownThickness.Value >= 0 )
            {
                formHasError = false;
                U2DConfig.Thickness = (double) numericUpDownThickness.Value;
                Redraw2dUnfolding();
            }
            else
            {
                formHasError = true;
                MessageBox.Show("Thickness must be positive.");
                numericUpDownThickness.Focus();
            }
        }

        private void numericUpDownKFactor_ValueChanged(object sender, EventArgs e)
        {
            if ( ( numericUpDownKFactor.Value >= 0 ) && ( numericUpDownKFactor.Value <= 1 ) )
            {
                formHasError = false;
                U2DConfig.KFactorValue = (double) numericUpDownKFactor.Value;
                Redraw2dUnfolding();
            }
            else
            {
                formHasError = true;
                MessageBox.Show("K-Factor must be positive and <= 1.");
                numericUpDownKFactor.Focus();
            }
        }

        private void numericUpDownInternalRadius_ValueChanged(object sender, EventArgs e)
        {
            if ( numericUpDownInternalRadius.Value >= 0 )
            {
                formHasError = false;
                Redraw2dUnfolding();
            }
            else
            {
                formHasError = true;
                MessageBox.Show("Internal radius must be positive.");
                numericUpDownInternalRadius.Focus();
            }
        }
        #endregion

        #region Button Events

        private void buttonSelectThicknessFromSegment_Click(object sender, EventArgs e)
        {
            // Pick segment
            /*     PromptEntityOptions PolyOptions = new PromptEntityOptions("Pick segment defining thickness :");
                 PolyOptions.SetRejectMessage("\nThis is not a generated sheet profile.\nSheet Profile is generated from a polyline with SHP command.\nCreate a sheet profile and try again.");
                 PolyOptions.AddAllowedClass(typeof(Polyline) , true);

                 PromptEntityResult PolyResult = Application.DocumentManager.MdiActiveDocument.Editor.GetEntity(PolyOptions);
                 if (PolyResult.Status != PromptStatus.OK)
                 {
                   return;
                 }*/

            /* U2DConfig.Thickness = */
            Unfold2D.GetSheetProfileThickness(/*PolyResult.ObjectId , PolyResult.PickedPoint*/);
            // numericUpDownThickness.Value = (decimal)U2DConfig.Thickness;
            Redraw2dUnfolding();
        }

        private void buttonSelectBendDeductionTableFile_Click(object sender, EventArgs e)
        {
            Ofd = new OpenFileDialog();
            string dir = "";

            if ( Ofd.ShowDialog() == DialogResult.OK )
            {
                dir = Ofd.FileName.Remove(Ofd.FileName.LastIndexOf('\\'));
                //textBoxBendDeductionTableFile.Text = Ofd.FileName.Remove(0 , (Ofd.FileName.LastIndexOf("\\")+1));
                textBoxBendDeductionTableFile.Text = Ofd.FileName;
            }

            if ( !Directory.Exists(dir) )
            {
                // Show error message and return...
                textBoxBendDeductionTableFile.Focus();
                return;
            }

            U2DConfig.BendDeductionTableFile = Ofd.FileName;
        }

        private void buttonSelectKFactorTableFile_Click(object sender, EventArgs e)
        {
            Ofd = new OpenFileDialog();
            string dir = "";
            if ( Ofd.ShowDialog() == DialogResult.OK )
            {
                dir = Ofd.FileName.Remove(Ofd.FileName.LastIndexOf('\\'));
                //textBoxKFactorTableFile.Text = Ofd.FileName.Remove(0 , (Ofd.FileName.LastIndexOf("\\") + 1));
                textBoxKFactorTableFile.Text = Ofd.FileName;
            }

            if ( !Directory.Exists(dir) )
            {
                // Show error message and return...
                textBoxKFactorTableFile.Focus();
                return;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if ( radioButtonMethodIsBendDeduction.Checked && textBoxBendDeductionTableFile.Text.Length < 1 )
            {
                MessageBox.Show(
                    Resources.PickUpBendDeductionTable,
                    Resources.Hint,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if ( radioButtonMethodIsKFactor.Checked && numericUpDownKFactor.Value < (decimal) 0.001 )
            {
                MessageBox.Show(
                   Resources.SetKFactor,
                   Resources.Hint,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information);
                return;
            }

            try
            {
                SaveSettings();
                DialogResult = DialogResult.OK;
                Unfold2D.ClearEvent();
            }
            catch
            {
                DialogResult = DialogResult.OK;
                // SaveSettings();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Unfold2D.ClearEventA();
            Close();
        }
        #endregion

        #region CheckBox Events
        private void checkBoxDrawCoating_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBoxDrawCoating.Checked )
            {
                groupBoxCoating.Enabled = true;
                //U2DConfig.DrawCoating = true;
                comboBoxCoatingColor.Enabled = true;
                comboBoxCoatingLayer.Enabled = true;
            }
            else
            {
                groupBoxCoating.Enabled = false;
                //U2DConfig.DrawCoating = false;
                comboBoxCoatingColor.Enabled = false;
                comboBoxCoatingLayer.Enabled = false;
            }

            Redraw2dUnfolding();
        }

        private void checkBoxDrawDimensions_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBoxDrawDimensions.Checked )
            {
                comboBoxDimensionColor.Enabled = true;
                comboBoxDimensionLayer.Enabled = true;
                comboBoxDimensionStyle.Enabled = true;
                U2DConfig.DrawDimensions = true;
            }
            else
            {
                comboBoxDimensionColor.Enabled = false;
                comboBoxDimensionLayer.Enabled = false;
                comboBoxDimensionStyle.Enabled = false;
                U2DConfig.DrawDimensions = false;
            }
            Redraw2dUnfolding();
        }

        #endregion

        #region Combos Events
        private void comboBoxCoatingColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            U2DConfig.CoatingColor = (System.Drawing.Color) comboBoxCoatingColor.SelectedItem;
            Redraw2dUnfolding();
        }
        private void comboBoxCoatingLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            U2DConfig.CoatingLayer = comboBoxCoatingLayer.Text;
            Redraw2dUnfolding();
        }

        private void comboBoxDimensionStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            U2DConfig.DimensionStyle = comboBoxDimensionStyle.Text;
            Redraw2dUnfolding();
        }

        private void comboBoxDimensionLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            U2DConfig.DimensionsLayer = comboBoxDimensionLayer.Text;
            Redraw2dUnfolding();
        }

        private void comboBoxDimensionColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            U2DConfig.DimensionsColor = (System.Drawing.Color) comboBoxDimensionColor.SelectedItem;
            Redraw2dUnfolding();
        }
        #endregion

        #region Validating Events
        #endregion

        public void SaveSettings()
        {
            Settings.Default.U2D_Thickness = numericUpDownThickness.Value;
            Settings.Default.U2D_InternalRadius = numericUpDownInternalRadius.Value;

            Settings.Default.U2D_MethodIsBendDeduction = radioButtonMethodIsBendDeduction.Checked;
            Settings.Default.U2D_BendDeductionTableFile = textBoxBendDeductionTableFile.Text;

            Settings.Default.U2D_MethodIsKFactor = radioButtonMethodIsKFactor.Checked;
            Settings.Default.U2D_KFactorTableFile = textBoxKFactorTableFile.Text;
            Settings.Default.U2D_KFactor = numericUpDownKFactor.Value;
            Settings.Default.U2D_KFactorIsPredefined = radioButtonKFactorUniform.Checked;

            Settings.Default.U2D_DrawDimensions = checkBoxDrawDimensions.Checked;

            // coating side
            if ( radioButtonCoatingLeft.Checked )
            {
                Settings.Default.U2D_CoatingSide = 1;
            }
            else if ( radioButtonCoatingRight.Checked )
            {
                Settings.Default.U2D_CoatingSide = 2;
            }
            else if ( radioButtonCoatingBoth.Checked )
            {
                Settings.Default.U2D_CoatingSide = 3;
            }

            Settings.Default.U2D_DrawCoating = checkBoxDrawCoating.Checked;
            Settings.Default.U2D_CoatingColor = (System.Drawing.Color) comboBoxCoatingColor.SelectedItem;
            Settings.Default.U2D_CoatingLayer = comboBoxCoatingLayer.Text;

            Settings.Default.U2D_DrawDimensions = checkBoxDrawDimensions.Checked;
            Settings.Default.U2D_DimensionColor = (System.Drawing.Color) comboBoxDimensionColor.SelectedItem;
            Settings.Default.U2D_DimensionLayer = comboBoxDimensionLayer.Text;
            Settings.Default.U2D_DimensionStyle = comboBoxDimensionStyle.Text;

            Settings.Default.Save();
        }

        private void Redraw2dUnfolding()
        {
            if ( formIsInitialized && !formHasError )
            {
                U2D_PREPARE_REDRAW();

                #region Setup Unfold Config

                U2DConfig.Thickness = (double) numericUpDownThickness.Value;

                #region unfolding method
                if ( radioButtonMethodIsBendDeduction.Checked && !radioButtonMethodIsKFactor.Checked )
                {
                    U2DConfig.UnfoldingMethod = Unfold2D.UnfoldingMethod.BendDeduction;
                    if ( !File.Exists(textBoxBendDeductionTableFile.Text) )
                    {
                        return;
                    }
                    else
                    {
                        U2DConfig.BendDeductionTableFile = textBoxBendDeductionTableFile.Text;
                    }
                }
                else
                {
                    U2DConfig.UnfoldingMethod = Unfold2D.UnfoldingMethod.KFactor;
                    if ( !radioButtonKFactorUniform.Checked )
                    {
                        U2DConfig.BendDeductionTableFile = "None";
                        if ( !File.Exists(textBoxKFactorTableFile.Text) )
                        {
                            return;
                        }
                        else
                        {
                            // Why this is empty?
                        }
                    }
                }
                #endregion

                #region coating

                U2DConfig.DrawCoating = checkBoxDrawCoating.Checked;

                if ( radioButtonCoatingLeft.Checked )
                {
                    U2DConfig.CoatingSide = SheetProfile.CoatingSide.Left;
                }
                else if ( radioButtonCoatingRight.Checked )
                {
                    U2DConfig.CoatingSide = SheetProfile.CoatingSide.Right;
                }
                else if ( radioButtonCoatingBoth.Checked )
                {
                    U2DConfig.CoatingSide = SheetProfile.CoatingSide.Both;
                }

                U2DConfig.CoatingLayer = comboBoxCoatingLayer.Text;

                try
                {
                    if ( comboBoxCoatingColor.SelectedItem.ToString() == "ByLayer" )
                    {
                        U2DConfig.CoatingColor = _drawingHelper.LayerManipulator.GetLayerColor(comboBoxCoatingLayer.Text);
                    }
                    else
                    {
                        U2DConfig.CoatingColor = (System.Drawing.Color) comboBoxCoatingColor.SelectedItem;
                    }
                }
                catch
                {
                    U2DConfig.CoatingColor = System.Drawing.Color.Yellow;
                }
                #endregion

                #region dimensions
                U2DConfig.DrawDimensions = checkBoxDrawDimensions.Checked;
                U2DConfig.DimensionStyle = comboBoxDimensionStyle.Text;
                U2DConfig.DimensionsLayer = comboBoxDimensionLayer.Text;
                try
                {
                    if ( comboBoxDimensionColor.SelectedItem.ToString() == "ByLayer" )
                    {
                        U2DConfig.DimensionsColor = _drawingHelper.LayerManipulator.GetLayerColor(comboBoxDimensionLayer.Text);
                    }
                    else
                    {
                        U2DConfig.DimensionsColor = (System.Drawing.Color) comboBoxDimensionColor.SelectedItem;
                    }
                }
                catch
                {
                    U2DConfig.DimensionsColor = System.Drawing.Color.Red;
                }
                #endregion

                #endregion

                Unfold2D.Draw2DUnfolding(U2DConfig);
                numericUpDownInternalRadius.Value = Settings.Default.U2D_InternalRadius;
            }
        }

        private void textBoxBendDeductionTableFile_TextChanged(object sender, EventArgs e)
        {
            Redraw2dUnfolding();
        }

        private void textBoxKFactorTableFile_TextChanged(object sender, EventArgs e)
        {
            Redraw2dUnfolding();
        }

        private void Unfold2dForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if ( e.KeyChar == (char) 27 )
            {
                Close();
            }
        }
    }
}