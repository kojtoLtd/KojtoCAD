using System;
using System.Windows.Forms;
using KojtoCAD.Properties;

namespace KojtoCAD.BlockItems.GlassUnits.Triple
{
  public partial class ImportTripleGlassUnitForm : Form
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

    #region Gaps Thickness
    /// <summary>
    /// Return the gap thickness of the double glass unit.
    /// </summary>
    public double InnerGapThickness
    {
      get
      {
        return (double)numericUpDownInnerGapThickness.Value;
      }
    }
    public double OuterGapThickness
    {
      get
      {
        return (double)numericUpDownOuterGapThickness.Value;
      }
    }
    #endregion

    #region Middle Glass Thickness
    public double MiddleGlassThickness
    {
      get
      {
        return (double)numericUpDownMiddleGlassThickness.Value;
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

    #region OnLoad Events

    public ImportTripleGlassUnitForm()
    {
      InitializeComponent();
    }

    private void ImportTripleGlassUnitForm_Load(object sender , EventArgs e)
    {
      try
      {
        #region Inner Glass Settings
        if (Settings.Default.TGU_InnerGlassIsLaminated)
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

        if (Settings.Default.TGU_InnerGlassHasFoil)
        {
          checkBoxInnerGlassHasFoil.Checked = true;
        }

        numericUpDownInnerInternalGlassThickness.Value = Settings.Default.TGU_InnerInternalGlassThickness;
        numericUpDownInnerPVBThickness.Value = Settings.Default.TGU_InnerPVBThickness;
        numericUpDownInnerExternalGlassThickness.Value = Settings.Default.TGU_InnerExternalGlassThickness;

        numericUpDownInnerGlassThickness.Value = Settings.Default.TGU_InnerGlassThickness;

        #endregion

        #region Gaps Settings
        numericUpDownInnerGapThickness.Value = Settings.Default.TGU_InnerGapThickness;
        numericUpDownOuterGapThickness.Value = Settings.Default.TGU_OuterGapThickness;
        #endregion

        #region Middle Glass settings
        numericUpDownMiddleGlassThickness.Value = Settings.Default.TGU_MiddleGlassThickness;
        #endregion

        #region Outer Glass Settings
        if (Settings.Default.TGU_OuterGlassIsLaminated)
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

        if (Settings.Default.TGU_OuterGlassHasFoil)
        {
          checkBoxOuterGlassHasFoil.Checked = true;
        }

        numericUpDownOuterInternalGlassThickness.Value = Settings.Default.TGU_OuterInternalGlassThickness;
        numericUpDownOuterPVBThickness.Value = Settings.Default.TGU_OuterPVBThickness;
        numericUpDownOuterExternalGlassThickness.Value = Settings.Default.TGU_OuterExternalGlassThickness;

        numericUpDownOuterGlassThickness.Value = Settings.Default.TGU_OuterGlassThickness;

        #endregion

        #region Profile Lenght Settings
        numericUpDownProfileLength.Value = Settings.Default.TGU_ProfileLenght;
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
        MessageBox.Show(Ex.Message + "\n" + Ex.Source  + "\n" + Ex.StackTrace);
      }

      RecalcTotalThickness();
    }
  
    #endregion

    #region Button Events

    private void buttonDrawGlassUnit_Click(object sender , EventArgs e)
    {
      SaveSettings( );
      DialogResult = DialogResult.OK;
    }

    private void buttonCancel_Click(object sender , EventArgs e)
    {
      Close();
    }

    #endregion

    #region Numeric DropDown Events

    private void numericUpDownInnerExternalPVB_ValueChanged(object sender , EventArgs e)
    {
      //= xx [mm]
      //string PvbThickness = ;
      double thickness = (double)numericUpDownInnerPVBThickness.Value * 0.38;
      string PvbThickness = "= " + thickness.ToString() + " [mm]";
      labelInnerLaminatedPVBLayers.Text = PvbThickness;
      RecalcTotalThickness();
    }

    private void numericUpDownOuterExternalPVB_ValueChanged(object sender , EventArgs e)
    {
      //= xx [mm]
      //string PvbThickness = ;
      double thickness = (double)numericUpDownOuterPVBThickness.Value * 0.38;
      string PvbThickness = "= " + thickness.ToString() + " [mm]";
      labelIOuterLaminatedPVBLayers.Text = PvbThickness;
      RecalcTotalThickness();
    }

    private void numericUpDownInnerExternalD_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownInnerExternalD2_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownInnerInternalD_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownInnerGap_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownMiddleD_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownOuterInternalD_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownOuterExternalD_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownOuterExternalD2_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownOuterGlassGap_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    private void numericUpDownLenghtOfProfile_ValueChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
    }

    #endregion

    #region Radiobutton Events

    private void radioButtonInnerMonolitic_CheckedChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();
      if (radioButtonInnerGlassIsMonolitic.Checked)
      {
        numericUpDownInnerGlassThickness.Enabled = true;

        numericUpDownInnerExternalGlassThickness.Enabled = false;
        numericUpDownInnerInternalGlassThickness.Enabled = false;
        numericUpDownInnerPVBThickness.Enabled = false;
      }
    }

    private void radioButtonInnerLaminated_CheckedChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();

      if (radioButtonInnerGlassIsLaminated.Checked)
      {
        numericUpDownInnerGlassThickness.Enabled = false;

        numericUpDownInnerExternalGlassThickness.Enabled = true;
        numericUpDownInnerInternalGlassThickness.Enabled = true;
        numericUpDownInnerPVBThickness.Enabled = true;
      }
    }

    private void radioButtonOuterMonolitic_CheckedChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();

      if (radioButtonOuterGlassIsMonolitic.Checked)
      {
        numericUpDownOuterGlassThickness.Enabled = true;

        numericUpDownOuterExternalGlassThickness.Enabled = false;
        numericUpDownOuterInternalGlassThickness.Enabled = false;
        numericUpDownOuterPVBThickness.Enabled = false;
      }
    }

    private void radioButtonOuterLaminated_CheckedChanged(object sender , EventArgs e)
    {
      RecalcTotalThickness();

      if (radioButtonOuterGlassIsLaminated.Checked)
      {
        numericUpDownOuterGlassThickness.Enabled = false;

        numericUpDownOuterExternalGlassThickness.Enabled = true;
        numericUpDownOuterInternalGlassThickness.Enabled = true;
        numericUpDownOuterPVBThickness.Enabled = true;
      }
    }

    #endregion
     
    private void RecalcTotalThickness()
    {
      double innerGlassThickness = 0.0;
      double outerGlassThickness = 0.0;
      double middleGlassThickness;
      double gapsThickness = 0.0;
      double totalThickness = 0.0;

      if (radioButtonInnerGlassIsMonolitic.Checked && !radioButtonInnerGlassIsLaminated.Checked)
      {
        innerGlassThickness = (double)numericUpDownInnerGlassThickness.Value;
      }
      else
      {
        innerGlassThickness = (double)numericUpDownInnerExternalGlassThickness.Value + (double)numericUpDownInnerInternalGlassThickness.Value + (double)numericUpDownInnerPVBThickness.Value * 0.38;
      }

      middleGlassThickness = (double)numericUpDownMiddleGlassThickness.Value;

      if (radioButtonOuterGlassIsMonolitic.Checked && !radioButtonOuterGlassIsLaminated.Checked)
      {
        outerGlassThickness = (double)numericUpDownOuterGlassThickness.Value;
      }
      else
      {
        outerGlassThickness = (double)numericUpDownOuterExternalGlassThickness.Value + (double)numericUpDownOuterInternalGlassThickness.Value + (double)numericUpDownOuterPVBThickness.Value * 0.38;
      }

      gapsThickness = (double)numericUpDownInnerGapThickness.Value + (double)numericUpDownOuterGapThickness.Value;

      totalThickness = innerGlassThickness + middleGlassThickness + outerGlassThickness + gapsThickness;

      labelTotalThickness.Text = "Total thickness = " + totalThickness.ToString() + " [mm]";
    }

    private void SaveSettings()
    {
      #region Inner Glass settings

      Settings.Default.TGU_InnerGlassIsLaminated = radioButtonInnerGlassIsLaminated.Checked;

      Settings.Default.TGU_InnerGlassThickness = numericUpDownInnerGlassThickness.Value;

      Settings.Default.TGU_InnerInternalGlassThickness = numericUpDownInnerInternalGlassThickness.Value;
      Settings.Default.TGU_InnerPVBThickness = numericUpDownInnerPVBThickness.Value;
      Settings.Default.TGU_InnerExternalGlassThickness = numericUpDownInnerExternalGlassThickness.Value;

      Settings.Default.TGU_InnerGlassHasFoil = checkBoxInnerGlassHasFoil.Checked;

      #endregion

      #region Gaps
      Settings.Default.TGU_InnerGapThickness = numericUpDownInnerGapThickness.Value;
      Settings.Default.TGU_OuterGapThickness = numericUpDownOuterGapThickness.Value;
      #endregion

      #region Middle Glass settings
      Settings.Default.TGU_MiddleGlassThickness = numericUpDownMiddleGlassThickness.Value;
      #endregion

      #region Outer Glass settings
      Settings.Default.TGU_OuterGlassIsLaminated = radioButtonOuterGlassIsLaminated.Checked;

      Settings.Default.TGU_OuterGlassThickness = numericUpDownOuterGlassThickness.Value;

      Settings.Default.TGU_OuterInternalGlassThickness = numericUpDownOuterInternalGlassThickness.Value;
      Settings.Default.TGU_OuterPVBThickness = numericUpDownOuterPVBThickness.Value;
      Settings.Default.TGU_OuterExternalGlassThickness = numericUpDownOuterExternalGlassThickness.Value;

      Settings.Default.TGU_OuterGlassHasFoil = checkBoxOuterGlassHasFoil.Checked;
      #endregion

      #region Profile lenght
      Settings.Default.TGU_ProfileLenght = numericUpDownProfileLength.Value;
      #endregion

      Settings.Default.Save();

    }

    private void ImportTripleGlassUnitForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        // The user pressed ESC - then close the form.
        if (e.KeyChar == (char)27)
        {
            Close();
        }
    }
  }
}
