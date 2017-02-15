using System;
using System.Collections.Specialized;
using System.Windows.Forms;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using XPTable.Editors;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
#else
using Teigha.DatabaseServices;
using Application = Bricscad.ApplicationServices.Application;
using Bricscad.ApplicationServices;
#endif

namespace KojtoCAD.RegionCommands
{
    public partial class RegionDescriptionForm : Form
    {
        private RegionMassProperties RMS;
        private readonly RegionMassProperties bakRMS;
        private readonly Document _doc;

        public RegionDescriptionForm()
        {
            InitializeComponent();
        }

        public RegionDescriptionForm(RegionMassProperties aRMS, Document doc)
        {
            InitializeComponent();
            RMS = aRMS;
            bakRMS = aRMS;
            _doc = doc;
        }

        private void RegionDescriptionForm_Load(object sender, EventArgs e)
        {
            tableRegDescr.BeginUpdate();

            string[] Units =
            {
                UnitsValue.Millimeters.ToString(),
                UnitsValue.Centimeters.ToString(),
                UnitsValue.Decimeters.ToString(),
                UnitsValue.Meters.ToString(),
                UnitsValue.Inches.ToString()
            };


            ComboBoxCellEditor UnitsEditor = new ComboBoxCellEditor();
            UnitsEditor.DropDownStyle = DropDownStyle.DropDownList;
            UnitsEditor.Items.AddRange(Units);
            tableRegDescr.ColumnModel.Columns[2].Editor = UnitsEditor;

            foreach (string str in Units)
            {
                comboUnits.Items.Add(str);
            }

            string sysUnits = (Application.GetSystemVariable("INSUNITS")).ToString();
            switch (sysUnits)
            {
                case "1":
                    sysUnits = "Inches";
                    break;
                case "2":
                    sysUnits = "Feet";
                    break;
                case "4":
                    sysUnits = "Millimeters";
                    break;
                case "5":
                    sysUnits = "Centimeters";
                    break;
                case "6":
                    sysUnits = "Meters";
                    break;
                case "14":
                    sysUnits = "Decimeters";
                    break;
                default:
                    MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                    buttonCancel_Click(sender, e);
                    break;
            }

            comboUnits.Text = sysUnits;
            comboUnits.SelectedIndexChanged += comboUnits_SelectedIndexChanged;

            #region filling the table

            // area
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Area"), new XPTable.Models.Cell(RMS.Area),
                    new XPTable.Models.Cell(sysUnits /*RMS.AreaUnit.ToString()*/), new XPTable.Models.Cell(" 2 "),
                    new XPTable.Models.Cell("")
                }));

            // Iy
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Iy"), new XPTable.Models.Cell(RMS.Iy),
                    new XPTable.Models.Cell(sysUnits /*RMS.IyUnit.ToString()*/), new XPTable.Models.Cell(" 4 "),
                    new XPTable.Models.Cell(" Moment of Inertia")
                }));

            // Wy_Upper
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Wy_Upper"), new XPTable.Models.Cell(RMS.WyUpper),
                    new XPTable.Models.Cell(sysUnits /*RMS.Wy_UpperUnit.ToString()*/), new XPTable.Models.Cell(" 3 "),
                    new XPTable.Models.Cell(" Section Modulus")
                }));

            // Wy_Lower
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Wy_Lower"), new XPTable.Models.Cell(RMS.WyLower),
                    new XPTable.Models.Cell(sysUnits /*RMS.Wy_LowerUnit.ToString()*/), new XPTable.Models.Cell(" 3 "),
                    new XPTable.Models.Cell(" Section Modulus")
                }));

            // D_Upper
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" D_Upper"), new XPTable.Models.Cell(RMS.DUpper),
                    new XPTable.Models.Cell(sysUnits /*RMS.D_UpperUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Distance")
                }));

            // D_Lower
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" D_Lower"), new XPTable.Models.Cell(RMS.DLower),
                    new XPTable.Models.Cell(sysUnits /*RMS.D_LowerUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Distance")
                }));

            // Iyy
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Iyy"), new XPTable.Models.Cell(RMS.Iyy),
                    new XPTable.Models.Cell(sysUnits /*RMS.IyyUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Radii of gyration")
                }));

            // Iz
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Iz"), new XPTable.Models.Cell(RMS.Iz),
                    new XPTable.Models.Cell(sysUnits /*RMS.IzUnit.ToString()*/), new XPTable.Models.Cell(" 4 "),
                    new XPTable.Models.Cell(" Moment of Inertia")
                }));

            // Wz_Right
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Wz_Right"), new XPTable.Models.Cell(RMS.WzRight),
                    new XPTable.Models.Cell(sysUnits /*RMS.Wz_RightUnit.ToString()*/), new XPTable.Models.Cell(" 3 "),
                    new XPTable.Models.Cell(" Section Modulus")
                }));

            // Wz_Left
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Wz_Left"), new XPTable.Models.Cell(RMS.WzLeft),
                    new XPTable.Models.Cell(sysUnits /*RMS.Wz_LeftUnit.ToString()*/), new XPTable.Models.Cell(" 3 "),
                    new XPTable.Models.Cell(" Section Modulus")
                }));

            // D_Right
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" D_Right"), new XPTable.Models.Cell(RMS.DRight),
                    new XPTable.Models.Cell(sysUnits /*RMS.D_RightUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Distance")
                }));

            // D_Left
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" D_Left"), new XPTable.Models.Cell(RMS.DLeft),
                    new XPTable.Models.Cell(sysUnits /*RMS.D_LeftUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Distance")
                }));

            // Izz
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Izz"), new XPTable.Models.Cell(RMS.Izz),
                    new XPTable.Models.Cell(sysUnits /*RMS.IzzUnit.ToString()*/), new XPTable.Models.Cell(" 1 "),
                    new XPTable.Models.Cell(" Radii of gyration")
                }));

            // Imin
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Imin"), new XPTable.Models.Cell(RMS.Imin),
                    new XPTable.Models.Cell(sysUnits /*RMS.IminUnit.ToString()*/), new XPTable.Models.Cell(" 4 "),
                    new XPTable.Models.Cell(" Principal moment")
                }));

            // Imax
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Imax"), new XPTable.Models.Cell(RMS.Imax),
                    new XPTable.Models.Cell(sysUnits /*RMS.ImaxUnit.ToString()*/), new XPTable.Models.Cell(" 4 "),
                    new XPTable.Models.Cell(" Principal moment")
                }));

            // Density
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" Density"), new XPTable.Models.Cell(RMS.Density),
                    new XPTable.Models.Cell("kg / cubic meter" /* + sysUnits RMS.DensityUnit.ToString()*/),
                    new XPTable.Models.Cell(""), new XPTable.Models.Cell("")
                }));

            // M
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" m"), new XPTable.Models.Cell(RMS.G),
                    new XPTable.Models.Cell("kg / m" /*RMS.GUnit.ToString()*/), new XPTable.Models.Cell(""),
                    new XPTable.Models.Cell(" Mass per length")
                }));
            // G
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" G"), new XPTable.Models.Cell(RMS.G),
                    new XPTable.Models.Cell("kgf / m" /*RMS.GUnit.ToString()*/), new XPTable.Models.Cell(" ≈"),
                    new XPTable.Models.Cell(" Weight per length")
                }));
            tableRegDescr.TableModel.Rows.Add(
                new XPTable.Models.Row(new[]
                {
                    new XPTable.Models.Cell(" G"), new XPTable.Models.Cell(RMS.G*9.80665),
                    new XPTable.Models.Cell("N / m" /*RMS.GUnit.ToString()*/), new XPTable.Models.Cell(" ≈"),
                    new XPTable.Models.Cell(" Weight per length")
                }));

            int N = tableRegDescr.TableModel.Rows.Count;
            for (int i = 0; i < N; i++)
            {
                tableRegDescr.TableModel.Rows[i].Cells[0].Enabled = false;
                tableRegDescr.TableModel.Rows[i].Cells[1].Enabled = false;
                tableRegDescr.TableModel.Rows[i].Cells[3].Enabled = false;
                tableRegDescr.TableModel.Rows[i].Cells[4].Enabled = false;
            }
            tableRegDescr.TableModel.Rows[N - 1].Cells[2].Enabled = false;
            tableRegDescr.TableModel.Rows[N - 4].Cells[1].Enabled = true;
            tableRegDescr.TableModel.Rows[N - 4].Cells[2].Enabled = false;
            tableRegDescr.TableModel.Rows[N - 3].Cells[2].Enabled = false;
            tableRegDescr.TableModel.Rows[N - 2].Cells[2].Enabled = false;

            #endregion

            tableRegDescr.CellPropertyChanged += tableRegDescr_CellPropertyChanged;
            tableRegDescr.HeaderRenderer = new XPTable.Renderers.GradientHeaderRenderer();
            tableRegDescr.TableModel.RowHeight += 3;
            tableRegDescr.FullRowSelect = true;
            tableRegDescr.EndUpdate();

            radioKg.Checked = true;
            radioKg.CheckedChanged += radioKg_CheckedChanged;

            DocumentHelper _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            StringCollection DimStyles = _drawingHelper.GetDimensionStylesList();

            int currentIndex = 0;
            foreach (string dimStyle in DimStyles)
            {
                comboBoxDimensionStyle.Items.Add(dimStyle);
                if (Settings.Default.SPG_DimensionStyle == dimStyle)
                {
                    comboBoxDimensionStyle.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }

            //comboBoxDimensionStyle.Text = Application.DocumentManager.MdiActiveDocument.Database.Dimstyle;


            // RegionDescription.DrawText( ref RMS );    
        }

        void tableRegDescr_CellPropertyChanged(object sender, XPTable.Events.CellEventArgs e)
        {
            // MessageBox.Show( e.CellPos.Column + "\n" + e.CellPos.Row + "\n" + e.Cell.Text + "\n" +  tableRegDescr.TableModel.Rows[e.CellPos.Row].Cells[0].Text + "\n" + e.OldValue);

            if (e.Column == 1)
            {
                if (tableRegDescr.TableModel.Rows[e.CellPos.Row].Cells[0].Text == " Density")
                {
                    RMS.Density = Convert.ToDouble(tableRegDescr.TableModel.Rows[15].Cells[1].Data);
                    RMS.G = RMS.Density*RMS.LinearVolume;
                    tableRegDescr.TableModel.Rows[16].Cells[1].Data = RMS.G;
                    tableRegDescr.TableModel.Rows[17].Cells[1].Data = RMS.G;
                    tableRegDescr.TableModel.Rows[18].Cells[1].Data = RMS.G*9.80665;
                }

                return;
            }

            UnitsValue UnitToConvertTo = UnitsValue.Millimeters;
            foreach (UnitsValue aUnit in Enum.GetValues(typeof(UnitsValue)))
            {
                if (aUnit.ToString() == e.Cell.Text)
                {
                    UnitToConvertTo = aUnit;
                }
            }

            string checkStr = tableRegDescr.TableModel.Rows[e.CellPos.Row].Cells[0].Text;
            UnitsValue scaleUnitsValue = UnitsValue.Millimeters;
            string sysUnits = (Application.GetSystemVariable("INSUNITS")).ToString();
            switch (sysUnits)
            {
                case "1":
                    scaleUnitsValue = UnitsValue.Inches;
                    break;
                case "2":
                    scaleUnitsValue = UnitsValue.Feet;
                    break;
                case "4":
                    scaleUnitsValue = UnitsValue.Millimeters;
                    break;
                case "5":
                    scaleUnitsValue = UnitsValue.Centimeters;
                    break;
                case "6":
                    scaleUnitsValue = UnitsValue.Meters;
                    break;
                case "14":
                    scaleUnitsValue = UnitsValue.Decimeters;
                    break;
                default:
                    MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                    break;
            }
            switch (checkStr.Remove(0, 1) /*tableRegDescr.TableModel.Rows[e.CellPos.Row].Cells[0].Text*/)
            {
                case "Area":
                    RMS.AreaUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[0].Cells[1].Data = RMS.Area*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.AreaUnit), 2);
                    break;
                case "Iy":
                    RMS.IyUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[1].Cells[1].Data = RMS.Iy*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.IyUnit), 4);
                    break;
                case "Wy_Upper":
                    RMS.WyUpperUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[2].Cells[1].Data = RMS.WyUpper*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.WyUpperUnit), 3);
                    break;
                case "Wy_Lower":
                    RMS.WyLowerUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[3].Cells[1].Data = RMS.WyLower*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.WyLowerUnit), 3);
                    break;
                case "D_Upper":
                    RMS.DUpperUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[4].Cells[1].Data = RMS.DUpper*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.DUpperUnit), 1);
                    break;
                case "D_Lower":
                    RMS.DLowerUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[5].Cells[1].Data = RMS.DLower*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.DLowerUnit), 1);
                    break;
                case "Iyy":
                    RMS.IyyUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[6].Cells[1].Data = RMS.Iyy*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.IyyUnit), 1);
                    break;
                case "Iz":
                    RMS.IzUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[7].Cells[1].Data = RMS.Iz*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.IzUnit), 4);
                    break;
                case "Wz_Right":
                    RMS.WzRightUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[8].Cells[1].Data = RMS.WzRight*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.WzRightUnit), 3);
                    break;
                case "Wz_Left":
                    RMS.WzLeftUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[9].Cells[1].Data = RMS.WzLeft*
                                                                     Math.Pow(
                                                                         UnitsConverter.GetConversionFactor(
                                                                             scaleUnitsValue, RMS.WzLeftUnit), 3);
                    break;
                case "D_Right":
                    RMS.DRightUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[10].Cells[1].Data = RMS.DRight*
                                                                      Math.Pow(
                                                                          UnitsConverter.GetConversionFactor(
                                                                              scaleUnitsValue, RMS.DRightUnit), 1);
                    break;
                case "D_Left":
                    RMS.DLeftUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[11].Cells[1].Data = RMS.DLeft*
                                                                      Math.Pow(
                                                                          UnitsConverter.GetConversionFactor(
                                                                              scaleUnitsValue, RMS.DLeftUnit), 1);
                    break;
                case "Izz":
                    RMS.IzzUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[12].Cells[1].Data = RMS.Izz*
                                                                      Math.Pow(
                                                                          UnitsConverter.GetConversionFactor(
                                                                              scaleUnitsValue, RMS.IzzUnit), 1);
                    break;
                case "Imin":
                    RMS.IminUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[13].Cells[1].Data = RMS.Imin*
                                                                      Math.Pow(
                                                                          UnitsConverter.GetConversionFactor(
                                                                              scaleUnitsValue, RMS.IminUnit), 4);
                    break;
                case "Imax":
                    RMS.ImaxUnit = UnitToConvertTo;
                    tableRegDescr.TableModel.Rows[14].Cells[1].Data = RMS.Imax*
                                                                      Math.Pow(
                                                                          UnitsConverter.GetConversionFactor(
                                                                              scaleUnitsValue, RMS.ImaxUnit), 4);
                    break;
                // case "Density":          
                // RMS.DensityUnit = UnitToConvertTo;
                //  break;
                //   case "G":         
                //RMS.GUnit = UnitToConvertTo;
                //     break;
                //   case "LinearVolume":
                //     RMS.LinearVolumeUnit = UnitToConvertTo;
                //    break;
            }

            // RegionDescription.DrawText( ref RMS );    
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (checkDrawText.Checked)
            {
                string pre;
                if (radioKg.Checked)
                {
                    pre = "Kg";
                }
                else
                {
                    pre = radioG.Checked ? "g" : "t";
                }

                RegionDescription.DrawText(ref RMS, pre + "/m", pre + "f/m", _doc.Database);
            }
            if (checkDrawDim.Checked)
            {
                RegionDescription.Draw(ref RMS, comboBoxDimensionStyle.Text);
            }
            Settings.Default.SPG_DimensionStyle = comboBoxDimensionStyle.Text;
            DialogResult = DialogResult.OK;

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            RMS = bakRMS;
            ComboBox cb = (ComboBox) sender;
            int N = 0;
            double k = 1.0;
            switch (cb.Text)
            {
                case "Inches":
                    N = 1;
                    k = 25.4;
                    break;
                case "Feet":
                    N = 2;
                    k = 12.0*25.4;
                    break;
                case "Millimeters":
                    N = 4;
                    k = 1.0;
                    break;
                case "Centimeters":
                    N = 5;
                    k = 10.0;
                    break;
                case "Meters":
                    N = 6;
                    k = 1000.0;
                    break;
                case "Decimeters":
                    N = 14;
                    k = 100.0;
                    break;
                default:
                    break;
            }
            RMS.Density = Convert.ToDouble(tableRegDescr.TableModel.Rows[15].Cells[1].Data);
            RMS.LinearVolume = bakRMS.Area*k*k/(1000.0*1000.0);

            //MessageBox.Show(RMS.LinearVolume.ToString());

            RMS.G = RMS.Density*RMS.LinearVolume;

            Application.SetSystemVariable("INSUNITS", N);

            tableRegDescr.TableModel.Rows[0].Cells[1].Data = RMS.Area;
            tableRegDescr.TableModel.Rows[1].Cells[1].Data = RMS.Iy;
            tableRegDescr.TableModel.Rows[2].Cells[1].Data = RMS.WyUpper;
            tableRegDescr.TableModel.Rows[3].Cells[1].Data = RMS.WyLower;
            tableRegDescr.TableModel.Rows[4].Cells[1].Data = RMS.DUpper;
            tableRegDescr.TableModel.Rows[5].Cells[1].Data = RMS.DLower;
            tableRegDescr.TableModel.Rows[6].Cells[1].Data = RMS.Iyy;
            tableRegDescr.TableModel.Rows[7].Cells[1].Data = RMS.Iz;
            tableRegDescr.TableModel.Rows[8].Cells[1].Data = RMS.WzRight;
            tableRegDescr.TableModel.Rows[9].Cells[1].Data = RMS.WzLeft;
            tableRegDescr.TableModel.Rows[10].Cells[1].Data = RMS.DRight;
            tableRegDescr.TableModel.Rows[11].Cells[1].Data = RMS.DLeft;
            tableRegDescr.TableModel.Rows[12].Cells[1].Data = RMS.Izz;
            tableRegDescr.TableModel.Rows[13].Cells[1].Data = RMS.Imin;
            tableRegDescr.TableModel.Rows[14].Cells[1].Data = RMS.Imax;


            for (int i = 0; i < 15; i++)
            {
                tableRegDescr.TableModel.Rows[i].Cells[2].Text = cb.Text;
            }

            tableRegDescr.TableModel.Rows[16].Cells[1].Data = RMS.G;
            tableRegDescr.TableModel.Rows[17].Cells[1].Data = RMS.G;

            if (radioKg.Checked == true)
            {
                k = 1.0;
            }
            if (radioG.Checked == true)
            {
                k = 1.0/1000.0;
            }
            if (radioT.Checked == true)
            {
                k = 1000.0;
            }
            tableRegDescr.TableModel.Rows[18].Cells[1].Data = RMS.G*9.80665*k;

        }

        private void radioKg_CheckedChanged(object sender, EventArgs e)
        {
            if (radioKg.Checked == true)
            {
                tableRegDescr.TableModel.Rows[15].Cells[2].Text = "Kg / cubic meter";
                tableRegDescr.TableModel.Rows[16].Cells[2].Text = "Kg / m";
                tableRegDescr.TableModel.Rows[17].Cells[2].Text = "Kgf / m";
                comboUnits_SelectedIndexChanged(comboUnits, null);
            }
        }

        private void radioG_CheckedChanged(object sender, EventArgs e)
        {
            if (radioG.Checked == true)
            {
                tableRegDescr.TableModel.Rows[15].Cells[2].Text = "g / cubic meter";
                tableRegDescr.TableModel.Rows[16].Cells[2].Text = "g / m";
                tableRegDescr.TableModel.Rows[17].Cells[2].Text = "gf / m";
                comboUnits_SelectedIndexChanged(comboUnits, null);
            }
        }

        private void radioT_CheckedChanged(object sender, EventArgs e)
        {
            if (radioT.Checked == true)
            {
                tableRegDescr.TableModel.Rows[15].Cells[2].Text = "t / cubic meter";
                tableRegDescr.TableModel.Rows[16].Cells[2].Text = "t / m";
                tableRegDescr.TableModel.Rows[17].Cells[2].Text = "tf / m";
                comboUnits_SelectedIndexChanged(comboUnits, null);
            }
        }

        private void RegionDescriptionForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char) 27)
            {
                Close();
            }
        }
    }
}
