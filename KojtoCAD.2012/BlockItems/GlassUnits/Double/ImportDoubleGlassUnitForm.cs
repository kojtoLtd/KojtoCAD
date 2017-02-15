﻿using System;
using System.Windows.Forms;
using KojtoCAD.Properties;

namespace KojtoCAD.BlockItems.GlassUnits.Double
{
    /// <summary>
    /// Dialog for double glass unit
    /// </summary>
    public partial class ImportDoubleGlassUnitForm : Form
    {

        #region Properties

        #region Inner glass properties

        /// <summary>
        /// Thickness of inner glass - if monolitic
        /// </summary>
        public double InnerGlassThickness
        {
            get
            {
                return (double)numericUpDownInnerGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of inner internal glass
        /// </summary>
        public double InnerInternalGlassThickness
        {
            get
            {
                return (double)numericUpDownInnerInternalGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of inner external glass
        /// </summary>
        public double InnerExternalGlassThickness
        {
            get
            {
                return (double)numericUpDownInnerExternalGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of inner external laminating foil
        /// </summary>
        public double InnerPVBThickness
        {
            get
            {
                return (double)numericUpDownInnerPVBThickness.Value * 0.38;
            }
        }
        /// <summary>
        /// Returns true if inner glass has lamination
        /// </summary>
        public bool InnerGlassIsLaminated
        {
            get
            {
                return radioButtonInnerGlassIsLaminated.Checked;
            }
        }
        /// <summary>
        /// Returns true if inner glass has foil
        /// </summary>
        public bool InnerGlassHasFoil
        {
            get
            {
                return checkBoxInnerGlassHasFoil.Checked;
            }

        }
        #endregion

        #region Outer glass properties
        /// <summary>
        /// Thickness of Outer glass - if monolitic
        /// </summary>
        public double OuterGlassThickness
        {
            get
            {
                return (double)numericUpDownOuterGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of Outer internal glass
        /// </summary>
        public double OuterInternalGlassThickness
        {
            get
            {
                return (double)numericUpDownOuterInternalGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of Outer external glass
        /// </summary>
        public double OuterExternalGlassThickness
        {
            get
            {
                return (double)numericUpDownOuterExternalGlassThickness.Value;
            }
        }
        /// <summary>
        /// Thickness of Outer external laminating foil
        /// </summary>
        public double OuterPVBThickness
        {
            get
            {
                return (double)numericUpDownOuterPVBThickness.Value * 0.38;
            }
        }
        /// <summary>
        /// Returns true if Outer glass has lamination
        /// </summary>
        public bool OuterGlassIsLaminated
        {
            get
            {
                return radioButtonOuterGlassIsLaminated.Checked;
            }
        }
        /// <summary>
        /// Returns true if Outer glass has foil
        /// </summary>
        public bool OuterGlassHasFoil
        {
            get
            {
                return checkBoxOuterGlassHasFoil.Checked;
            }

        }
        #endregion

        #region Gap Thickness
        /// <summary>
        /// Return the gap thickness of the double glass unit.
        /// </summary>
        public double GapThickness
        {
            get
            {
                return (double)numericUpDownGapThickness.Value;
            }
        }
        #endregion

        #region Profile lenght

        /// <summary>
        /// Lenght of glass profile
        /// </summary>
        public double ProfileLength
        {
            get
            {
                return (double)numericUpDownProfileLength.Value;
            }
        }
        #endregion

        #endregion


        #region Constructor and OnLoad
        /// <summary>
        /// Dialog for double glass unit
        /// </summary>
        public ImportDoubleGlassUnitForm()
        {
            InitializeComponent();
        }

        private void ImportDoubleGlassUnitForm_Load(object sender, EventArgs e)
        {
            try
            {
                #region Inner Glass Settings
                if (Settings.Default.DGU_InnerGlassIsLaminated)
                {
                    radioButtonInnerGlassIsLaminated.Checked = true;

                    numericUpDownInnerGlassThickness.Enabled = false;


                }
                else
                {
                    radioButtonInnerGlassIsMonolitic.Checked = true;

                    numericUpDownInnerExternalGlassThickness.Enabled = false;
                    numericUpDownInnerInternalGlassThickness.Enabled = false;
                    numericUpDownInnerPVBThickness.Enabled = false;


                }

                if (Settings.Default.DGU_InnerGlassHasFoil)
                {
                    checkBoxInnerGlassHasFoil.Checked = true;
                }


                numericUpDownInnerInternalGlassThickness.Value = Settings.Default.DGU_InnerInternalGlassThickness;
                numericUpDownInnerPVBThickness.Value = Settings.Default.DGU_InnerPVBThickness;
                numericUpDownInnerExternalGlassThickness.Value = Settings.Default.DGU_InnerExternalGlassThickness;

                numericUpDownInnerGlassThickness.Value = Settings.Default.DGU_InnerGlassThickness;
                #endregion

                #region Gap Settings
                numericUpDownGapThickness.Value = Settings.Default.DGU_GapThickness;
                #endregion

                #region Outer Glass Settings
                if (Settings.Default.DGU_OuterGlassIsLaminated)
                {
                    radioButtonOuterGlassIsLaminated.Checked = true;
                    numericUpDownOuterGlassThickness.Enabled = false;
                }
                else
                {
                    radioButtonOuterGlassIsMonolitic.Checked = true;

                    numericUpDownOuterExternalGlassThickness.Enabled = false;
                    numericUpDownOuterInternalGlassThickness.Enabled = false;
                    numericUpDownOuterPVBThickness.Enabled = false;
                }

                if (Settings.Default.DGU_OuterGlassHasFoil)
                {
                    checkBoxOuterGlassHasFoil.Checked = true;
                }

                numericUpDownOuterInternalGlassThickness.Value = Settings.Default.DGU_OuterInternalGlassThickness;
                numericUpDownOuterPVBThickness.Value = Settings.Default.DGU_OuterPVBThickness;
                numericUpDownOuterExternalGlassThickness.Value = Settings.Default.DGU_OuterExternalGlassThickness;

                numericUpDownOuterGlassThickness.Value = Settings.Default.DGU_OuterGlassThickness;

                #endregion

                #region Profile Lenght Settings
                numericUpDownProfileLength.Value = Settings.Default.DGU_ProfileLenght;
                #endregion


                double thickness = (double)numericUpDownInnerPVBThickness.Value * 0.38;
                string PvbThickness = "= " + thickness.ToString() + " [mm]";
                labelInnerLaminatedPVBLayers.Text = PvbThickness;

                thickness = (double)numericUpDownOuterPVBThickness.Value * 0.38;
                PvbThickness = "= " + thickness.ToString() + " [mm]";
                labelIOuterLaminatedPVBLayers.Text = PvbThickness;

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
            }
            RecalcTotalThickness();
        }

        #endregion

        #region Buttons Events
        private void buttonDrawGlassUnit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SaveSettings();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion

        #region Numeric DropDowns Events

        private void numericUpDownInnerPVBThickness_ValueChanged(object sender, EventArgs e)
        {
            //= xx [mm]
            //string PvbThickness = ;
            double thickness = (double)numericUpDownInnerPVBThickness.Value * 0.38;
            string PvbThickness = "= " + thickness.ToString() + " [mm]";
            labelInnerLaminatedPVBLayers.Text = PvbThickness;
            RecalcTotalThickness();
        }

        private void numericUpDownOuterPVBThickness_ValueChanged(object sender, EventArgs e)
        {
            //= xx [mm]
            //string PvbThickness = ;
            double thickness = (double)numericUpDownOuterPVBThickness.Value * 0.38;
            string PvbThickness = "= " + thickness.ToString() + " [mm]";
            labelIOuterLaminatedPVBLayers.Text = PvbThickness;
            RecalcTotalThickness();
        }

        private void numericUpDownInnerExternalGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownInnerInternalGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownOuterInternalGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownOuterExternalGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownInnerGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownOuterGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownGapThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownLenghtOfProfile_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        #endregion

        #region Radiobuttons Events
        private void radioButtonInnerGlassIsMonolitic_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonInnerGlassIsMonolitic.Checked)
            {
                numericUpDownInnerGlassThickness.Enabled = true;

                numericUpDownInnerExternalGlassThickness.Enabled = false;
                numericUpDownInnerInternalGlassThickness.Enabled = false;
                numericUpDownInnerPVBThickness.Enabled = false;
            }

            RecalcTotalThickness();
        }

        private void radioButtonInnerGlassIsLaminated_CheckedChanged(object sender, EventArgs e)
        {


            if (radioButtonInnerGlassIsLaminated.Checked)
            {
                numericUpDownInnerGlassThickness.Enabled = false;

                numericUpDownInnerExternalGlassThickness.Enabled = true;
                numericUpDownInnerInternalGlassThickness.Enabled = true;
                numericUpDownInnerPVBThickness.Enabled = true;
            }
            RecalcTotalThickness();
        }

        private void radioButtonOuterGlassIsMonolitic_CheckedChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();

            if (radioButtonOuterGlassIsMonolitic.Checked)
            {
                numericUpDownOuterGlassThickness.Enabled = true;

                numericUpDownOuterInternalGlassThickness.Enabled = false;
                numericUpDownOuterExternalGlassThickness.Enabled = false;
                numericUpDownOuterPVBThickness.Enabled = false;
            }
        }

        private void radioButtonOuterGlassIsLaminated_CheckedChanged(object sender, EventArgs e)
        {


            if (radioButtonOuterGlassIsLaminated.Checked)
            {
                numericUpDownOuterGlassThickness.Enabled = false;

                numericUpDownOuterInternalGlassThickness.Enabled = true;
                numericUpDownOuterExternalGlassThickness.Enabled = true;
                numericUpDownOuterPVBThickness.Enabled = true;
            }
            RecalcTotalThickness();
        }
        #endregion

        #region CheckBoxes Events
        private void checkBoxInnerGlassHasFoil_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxOuterGlassHasFoil_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion

        private void RecalcTotalThickness()
        {
            try
            {
                double innerGlassThickness = 0.0;
                double outerGlassThickness = 0.0;
                double gapThickness = 0.0;
                double totalThickness = 0.0;

                if (radioButtonInnerGlassIsMonolitic.Checked)
                {
                    innerGlassThickness = (double)numericUpDownInnerGlassThickness.Value;
                }
                else
                {
                    innerGlassThickness = (double)numericUpDownInnerExternalGlassThickness.Value + (double)numericUpDownInnerInternalGlassThickness.Value + (double)numericUpDownInnerPVBThickness.Value * 0.38;
                }

                if (radioButtonOuterGlassIsMonolitic.Checked)
                {
                    outerGlassThickness = (double)numericUpDownOuterGlassThickness.Value;
                }
                else
                {
                    outerGlassThickness = (double)numericUpDownOuterInternalGlassThickness.Value + (double)numericUpDownOuterExternalGlassThickness.Value + (double)numericUpDownOuterPVBThickness.Value * 0.38;
                }

                gapThickness = (double)numericUpDownGapThickness.Value;

                totalThickness = innerGlassThickness + outerGlassThickness + gapThickness;

                labelTotalThickness.Text = "Total thickness = " + totalThickness.ToString() + " [mm]";
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);

            }
        }

        private void SaveSettings()
        {
            #region Inner Glass settings

            Settings.Default.DGU_InnerGlassIsLaminated = radioButtonInnerGlassIsLaminated.Checked;

            Settings.Default.DGU_InnerGlassThickness = numericUpDownInnerGlassThickness.Value;

            Settings.Default.DGU_InnerInternalGlassThickness = numericUpDownInnerInternalGlassThickness.Value;
            Settings.Default.DGU_InnerPVBThickness = numericUpDownInnerPVBThickness.Value;
            Settings.Default.DGU_InnerExternalGlassThickness = numericUpDownInnerExternalGlassThickness.Value;

            Settings.Default.DGU_InnerGlassHasFoil = checkBoxInnerGlassHasFoil.Checked;

            #endregion

            #region Gap
            Settings.Default.DGU_GapThickness = numericUpDownGapThickness.Value;
            #endregion

            #region Outer Glass settings
            Settings.Default.DGU_OuterGlassIsLaminated = radioButtonOuterGlassIsLaminated.Checked;

            Settings.Default.DGU_OuterGlassThickness = numericUpDownOuterGlassThickness.Value;

            Settings.Default.DGU_OuterInternalGlassThickness = numericUpDownOuterInternalGlassThickness.Value;
            Settings.Default.DGU_OuterPVBThickness = numericUpDownOuterPVBThickness.Value;
            Settings.Default.DGU_OuterExternalGlassThickness = numericUpDownOuterExternalGlassThickness.Value;

            Settings.Default.DGU_OuterGlassHasFoil = checkBoxOuterGlassHasFoil.Checked;
            #endregion

            #region Profile lenght
            Settings.Default.DGU_ProfileLenght = numericUpDownProfileLength.Value;
            #endregion

            Settings.Default.Save();

        }

        private void ImportDoubleGlassUnitForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }
    }
}