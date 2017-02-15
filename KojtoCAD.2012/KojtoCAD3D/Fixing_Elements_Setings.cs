using System;
using System.Windows.Forms;

namespace KojtoCAD.KojtoCAD3D
{
    public partial class Fixing_Elements_Setings : Form
    {
        private double A_;
        public double A { get { return (A_); } }

        private double B_;
        public double B { get { return (B_); } }

        public Fixing_Elements_Setings(bool pereferial = false)
        {
            if (!pereferial)
            {
                A_ = UtilityClasses.ConstantsAndSettings.fixing_A;
                B_ = UtilityClasses.ConstantsAndSettings.fixing_B;
            }
            else
            {
                A_ = UtilityClasses.ConstantsAndSettings.fixing_pA;
                B_ = UtilityClasses.ConstantsAndSettings.fixing_pB;
            }

            InitializeComponent();
            textBox_A.Text = string.Format(" {0:f2}", A_);
            textBox_B.Text = string.Format(" {0:f2}", B_);
            
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                A_ = double.Parse(textBox_A.Text);
                if (A_ <= 0.0)
                {
                    MessageBox.Show("Missing Corect A > 0.0 !", "E R R O R");
                    textBox_A.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect A !", "E R R O R");
                textBox_A.Focus();
                return;
            }

            try
            {
                B_ = double.Parse(textBox_B.Text);
                if (B_ <= 0.0)
                {
                    MessageBox.Show("Missing Corect B > 0.0 !", "E R R O R");
                    textBox_B.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect B !", "E R R O R");
                textBox_B.Focus();
                return;
            }
            

            this.DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button_HELP_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/FIXING_SETTINGS.htm");
        }
    }
}
