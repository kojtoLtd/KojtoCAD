using System;
using System.Windows.Forms;
using KojtoCAD.Properties;

namespace KojtoCAD.GraphicItems.Gear
{
  public partial class ImportGearForm : Form
  {
    public string DrawItem;
    public decimal gearModule
    {
      get
      {
        return numericUpDownGearModule.Value;
      }
    }
    public decimal gearTeethCount
    {
      get
      {
        return numericUpDownGearTeethCount.Value;
      }
    }


    public ImportGearForm()
    {
      InitializeComponent();
    }

    private void buttonDrawGear_Click(object sender, EventArgs e)
    {
      DrawItem = "GEAR";
      SaveSettings( );
      DialogResult = DialogResult.OK;
    }

    private void buttonDrawRack_Click(object sender, EventArgs e)
    {
      DrawItem = "RACK";
      SaveSettings( );
      DialogResult = DialogResult.OK;
    }

    private void SaveSettings ( )
    {
      Settings.Default.GearModule = numericUpDownGearModule.Value;
      Settings.Default.GearNumberOfTheeth = numericUpDownGearTeethCount.Value;
      Settings.Default.Save( );
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    private void numericUpDownGearModule_ValueChanged(object sender, EventArgs e)
    {
      Settings.Default.GearModule = numericUpDownGearModule.Value;
    }

    private void numericUpDownGearTeethCount_ValueChanged ( object sender, EventArgs e )
    {
      Settings.Default.GearNumberOfTheeth = numericUpDownGearTeethCount.Value;
    }

    private void ImportGearForm_Load ( object sender, EventArgs e )
    {
      numericUpDownGearModule.Value = Settings.Default.GearModule;
      numericUpDownGearTeethCount.Value = Settings.Default.GearNumberOfTheeth;
    }

    private void ImportGearForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        // The user pressed ESC - then close the form.
        if (e.KeyChar == (char)27)
        {
            Close();
        }
    }
  }
}