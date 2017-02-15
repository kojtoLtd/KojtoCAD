using System;
using System.Windows.Forms;
using KojtoCAD.Properties;

namespace KojtoCAD.BlockItems.GlassUnits.Single
{
    public partial class ImportSingleGlassUnitForm : Form
    {
        #region Properties

        public bool IsLaminated
        {
            get
            {
                return radioButtonLaminated.Checked;
            }
        }

        public double GlassThickness
        {
            get
            {
                return (double)numericUpDownGlassThickness.Value;
            }
        }

        public double InnerGlassThickness
        {
            get
            {
                return (double)numericUpDownInnerGlassThickness.Value;
            }
        }

        public double PVBLayersThickness
        {
            get
            {
                return (double)numericUpDownPVBThickness.Value * 0.38;
            }
        }

        public double OuterGlassThickness
        {
            get
            {
                return (double)numericUpDownOuterGlassThickness.Value;
            }
        }

        public double ProfileLenght
        {
            get
            {
                return (double)numericUpDownProfileLenght.Value;
            }
        }

        #endregion

        #region Constructor and OnLoad

        public ImportSingleGlassUnitForm()
        {
            InitializeComponent();
        }

        private void ImportSingleGlassUnitForm_Load(object sender, EventArgs e)
        {
            if (Settings.Default.SGU_IsLaminated)
            {
                radioButtonLaminated.Checked = true;
                numericUpDownGlassThickness.Enabled = false;
            }
            else
            {
                radioButtonMonolitic.Checked = true;

                numericUpDownInnerGlassThickness.Enabled = false;
                numericUpDownOuterGlassThickness.Enabled = false;
                numericUpDownPVBThickness.Enabled = false;
            }

            numericUpDownGlassThickness.Value = Settings.Default.SGU_GlassThickness;
            numericUpDownInnerGlassThickness.Value = Settings.Default.SGU_InnerGlassThickness;
            numericUpDownOuterGlassThickness.Value = Settings.Default.SGU_OuterGlassThickness;
            numericUpDownPVBThickness.Value = Settings.Default.SGU_PVBThickness;

            numericUpDownProfileLenght.Value = Settings.Default.SGU_ProfileLenght;


            double thickness = (double)numericUpDownPVBThickness.Value * 0.38;
            if (thickness < 1.0)
            {
                thickness = 1;
            }
            string pvbThickness = "= " + thickness.ToString() + " [mm]";
            labelInnerLaminatedPVBLayers.Text = pvbThickness;

            RecalcTotalThickness();
        }

        #endregion

        #region Radio Button Events
        private void radioButtonMonolitic_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownGlassThickness.Enabled = true;

            numericUpDownInnerGlassThickness.Enabled = false;
            numericUpDownOuterGlassThickness.Enabled = false;
            numericUpDownPVBThickness.Enabled = false;
            RecalcTotalThickness();
        }

        private void radioButtonLaminated_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownGlassThickness.Enabled = false;

            numericUpDownInnerGlassThickness.Enabled = true;
            numericUpDownOuterGlassThickness.Enabled = true;
            numericUpDownPVBThickness.Enabled = true;
            RecalcTotalThickness();
        }
        #endregion

        #region NumericUpDown Events
        private void numericUpDownGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownInnerGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownPVBThickness_ValueChanged(object sender, EventArgs e)
        {
            double thickness = (double)numericUpDownPVBThickness.Value * 0.38;
            string PvbThickness = "= " + thickness.ToString() + " [mm]";
            labelInnerLaminatedPVBLayers.Text = PvbThickness;
            RecalcTotalThickness();
        }

        private void numericUpDownOuterGlassThickness_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void numericUpDownProfileLenght_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }
        #endregion

        private void checkBoxGlassFoil_CheckedChanged(object sender, EventArgs e)
        {
            RecalcTotalThickness();
        }

        private void buttonDrawGlassUnit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void RecalcTotalThickness()
        {
            try
            {
                double glassThickness = 0.0;
                double totalThickness = 0.0;

                if (radioButtonMonolitic.Checked) // && !radioButtonLaminated.Checked
                {
                    glassThickness = (double)numericUpDownGlassThickness.Value;
                }
                else if (radioButtonLaminated.Checked)
                {
                    glassThickness = (double)numericUpDownInnerGlassThickness.Value + (double)numericUpDownOuterGlassThickness.Value + (double)numericUpDownPVBThickness.Value * 0.38;
                }

                totalThickness = glassThickness;

                labelTotalThickness.Text = "Total thickness = " + totalThickness.ToString() + " [mm]";
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
            }
        }

        private void SaveSettings()
        {
            Settings.Default.SGU_IsLaminated = radioButtonLaminated.Checked;

            Settings.Default.SGU_GlassThickness = numericUpDownGlassThickness.Value;
            Settings.Default.SGU_InnerGlassThickness = numericUpDownInnerGlassThickness.Value;
            Settings.Default.SGU_OuterGlassThickness = numericUpDownOuterGlassThickness.Value;
            Settings.Default.SGU_PVBThickness = numericUpDownPVBThickness.Value;
            Settings.Default.SGU_ProfileLenght = numericUpDownProfileLenght.Value;
            Settings.Default.Save();
        }

        private void ImportSingleGlassUnitForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }
    }
}