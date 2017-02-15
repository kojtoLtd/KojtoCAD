using System;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using KojtoCAD.Properties;
using LumenWorks.Framework.IO.Csv;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Application = Bricscad.ApplicationServices.Application;

#endif

namespace KojtoCAD.GraphicItems.SheetProfile
{
  public partial class SheetDescriptionForm : Form
  {
    //constructors
    public SheetDescriptionForm()
    {
      InitializeComponent();
    }
    public SheetDescriptionForm(double thickness)
    {
      InitializeComponent();
      textBoxThickness.Text = thickness.ToString();
    }

    //public members
    public double GetThickness()
    {
      try
      {
        return Convert.ToDouble(textBoxThickness.Text);
      }
      catch
      {
        return -1.0;
      }
    }
    public int GetCoating()
    {
      int rez = -1;
      if (radioButtonCoatingLeft.Checked)
      {
        rez = 0;
      }
      else
      {
        if (radioButtonCoatingBoth.Checked)
        {
          rez = 1;
        }
        else
        {
          if (radioButtonCoatingRight.Checked)
          {
            rez = 2;
          }
        }
      }
      return rez;
    }

    public string GetCoatingLayer()
    {
      return comboBoxCoatingLayer.Text;
    }

    public System.Drawing.Color GetCoatingColor()
    {
      return (System.Drawing.Color)comboBoxCoatingColor.SelectedItem;
    }

    public double GetDensity()
    {
      double rez = -1;
      try
      {
        DataGridViewRow row = dataGridViewMaterials.SelectedRows[0];
        rez = Convert.ToDouble(row.Cells[1].Value.ToString());
      }
      catch
      {
      }
      return rez;
    }

    public string GetMaterial()
    {
      string rez = "";
      try
      {
        DataGridViewRow row = dataGridViewMaterials.SelectedRows[0];
        rez =row.Cells[0].Value.ToString();
      }
      catch
      {
      }
      return rez;
    }

    public bool GetCheckBoxDrawCoating()
    {
      return checkBoxDrawCoating.Checked;
    }

      //private members
    private void SheetDescriptionForm_Load(object sender , EventArgs e)
    {
      dataGridViewMaterials.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridViewMaterials.MultiSelect = false;

      ReadMaterialsCsv();

      Settings.Default.SHD_HasCoating = checkBoxDrawCoating.Checked;

      checkBoxDrawCoating.Checked = true;
      radioButtonCoatingLeft.Checked = true;

      #region Load Layers
      // Load layers
      //Editor Ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
      Database Db = Application.DocumentManager.MdiActiveDocument.Database;

      using (Transaction Tr = Db.TransactionManager.StartTransaction())
      {
        LayerTable LayerTable = (LayerTable)Tr.GetObject(Db.LayerTableId , OpenMode.ForRead);
        LayerTableRecord LayerRecord;

        comboBoxCoatingLayer.Sorted = true;
        comboBoxCoatingLayer.Items.Add("Current Layer");

        foreach (ObjectId LayerId in LayerTable)
        {
          LayerRecord = (LayerTableRecord)Tr.GetObject(LayerId , OpenMode.ForWrite);
          comboBoxCoatingLayer.Items.Add(LayerRecord.Name);
          if (LayerRecord.Name == Settings.Default.SHD_CoatingLayer)         
          {
            comboBoxCoatingLayer.SelectedText = LayerRecord.Name;
          }
        }
        Tr.Commit();
      }
      #endregion

      #region Load Colors
      
      System.Drawing.Color color = System.Drawing.Color.FromName("ByLayer");

      comboBoxCoatingColor.SortAlphabetically = true;
      comboBoxCoatingColor.Items.Insert(0 , color);

      for (int i = 0 ; i < comboBoxCoatingColor.Items.Count ; i++)
      {
        if ((System.Drawing.Color)comboBoxCoatingColor.Items[i] == Settings.Default.SHD_CoatingColor)
        {
          comboBoxCoatingColor.SelectedIndex = i;
        }
      }

      #endregion

      SheetDescription.RedrawDescription( );
    }

    private void dataGridViewMaterials_SelectionChanged(object sender , EventArgs e)
    {
      SheetDescription.RedrawDescription( );
    }

    #region RadioButton Events

    private void radioButtonCoatingLeft_CheckedChanged(object sender , EventArgs e)
    {
      if (radioButtonCoatingLeft.Checked)
      {
        SheetDescription.RedrawDescription();
      }
    }

    private void radioButtonCoatingBoth_CheckedChanged(object sender , EventArgs e)
    {
      if (radioButtonCoatingBoth.Checked)
      {
        SheetDescription.RedrawDescription();
      }
    }

    private void radioButtonCoatingRight_CheckedChanged(object sender , EventArgs e)
    {
      if (radioButtonCoatingRight.Checked)
      {
        SheetDescription.RedrawDescription();
      }
    } 
    #endregion

    #region Combobox Events

    private void comboBoxCoatingLayer_SelectedIndexChanged(object sender , EventArgs e)
    {
      SheetDescription.RedrawCoating();    
    }

    private void comboBoxCoatingColor_SelectedIndexChanged(object sender , EventArgs e)
    {
      SheetDescription.RedrawColor();
    }
    
    #endregion

    private void checkBoxDrawCoating_CheckedChanged(object sender , EventArgs e)
    {
      SheetDescription.RedrawDescription( );
    }

    private void buttonOK_Click(object sender , EventArgs e)
    {
      SaveSettings();
      DialogResult = DialogResult.OK;
    }

    private void buttonCancel_Click(object sender , EventArgs e)
    {
      SheetDescription.Clear();
    }

    private void ReadMaterialsCsv()
    {
      // open the file which is a CSV file with headers
      string CSVFile = Path.Combine( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.tablesDir,"materials.csv");
      using (CachedCsvReader csv = new  CachedCsvReader(new StreamReader(CSVFile), true))
      {
        // Field headers will automatically be used as column names
        dataGridViewMaterials.DataSource = csv;
      }
    }

    private void SaveSettings()
    {
      Settings.Default.SHD_CoatingColor = (System.Drawing.Color)comboBoxCoatingColor.SelectedItem;
      Settings.Default.SHD_CoatingLayer = comboBoxCoatingLayer.Text;

      // coating side
      if (radioButtonCoatingLeft.Checked)
      {
        Settings.Default.SHD_CoatingSide = 1;
      }
      else if (radioButtonCoatingRight.Checked)
      {
        Settings.Default.SHD_CoatingSide = 2;
      }
      else if (radioButtonCoatingBoth.Checked)
      {
        Settings.Default.SHD_CoatingSide = 3;
      }

      Settings.Default.SHD_Material = dataGridViewMaterials.CurrentRow.Cells[0].Value.ToString();

      Settings.Default.Save( );
    }

    private void SheetDescriptionForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        // The user pressed ESC - then close the form.
        if (e.KeyChar == (char)27)
        {
            Close();
        }
    }
  }
}