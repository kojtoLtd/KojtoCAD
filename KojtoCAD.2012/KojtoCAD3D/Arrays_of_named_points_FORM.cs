using System;
using System.Collections.Generic;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.Geometry;
#endif

namespace KojtoCAD.KojtoCAD3D
{
    public partial class Form_Arrays_of_named_points : Form
    {

        //public string Name { get; set; }    A.K.K. -> hides inherited member

        public Form_Arrays_of_named_points()
        {
            InitializeComponent(); 
        }
        public Form_Arrays_of_named_points(ref List<UtilityClasses.Pair<string, List<UtilityClasses.Pair<string, Point3d>>>> NamedPointsARRAYs)
        {
            InitializeComponent();
            if (NamedPointsARRAYs.Count > 0)
            {
                foreach (UtilityClasses.Pair<string, List<UtilityClasses.Pair<string, Point3d>>> item in NamedPointsARRAYs)
                {
                    comboBox_Names_List.Items.Add(item.First);
                }
            }
        }

        private void button_Help_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/NAMED_POINTS_DRAW.htm");
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                comboBox_Names_List.Text = comboBox_Names_List.Text.TrimStart();
                comboBox_Names_List.Text=comboBox_Names_List.Text.TrimEnd();

                if (comboBox_Names_List.Text.Length <= 0)
                {
                    MessageBox.Show("Missing Corect Array Name !", "E R R O R");
                    comboBox_Names_List.Focus();
                    return;
                }
                else
                {
                    bool valid = false;
                    foreach (char ch in comboBox_Names_List.Text)
                    {
                        if ((ch != ' ') && char.IsLetter(ch))
                        {
                            valid = true;
                            break;
                        }
                    }

                    Name = comboBox_Names_List.Text;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect A !", "E R R O R");
                //textBox_A.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
