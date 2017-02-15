using System;
using System.Windows.Forms;

namespace KojtoCAD.KojtoCAD3D
{
    public partial class Gaskets_FORM : Form
    {
        private double mA_;
        public double mA { get { return (mA_); } set { mA_ = value; } }
       
        private double mB_;
        public double mB { get { return (mB_); } set { mB_ = value; } }

        private double mC_;
        public double mC { get { return (mC_); } set { mC_ = value; } }

        public Gaskets_FORM()
        {
            InitializeComponent();

        }
        public Gaskets_FORM(double A, double B, double C)
        {
            InitializeComponent();

            mA = A; mB = B; mC = C;

            textBox_Gaskets_A.Text = string.Format("{0}", mA_);
            textBox_Gaskets_B.Text = string.Format("{0}", mB_);
            textBox_Gaskets_C.Text = string.Format("{0}", mC_);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                mA_ = double.Parse(textBox_Gaskets_A.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect A !", "E R R O R");
                textBox_Gaskets_A.Focus();
                return;
            }

            try
            {
                mB_ = double.Parse(textBox_Gaskets_B.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect B !", "E R R O R");
                textBox_Gaskets_B.Focus();
                return;
            }

            try
            {
                mC_ = double.Parse(textBox_Gaskets_C.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect C !", "E R R O R");
                textBox_Gaskets_C.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button_HELP_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/Gaskets_Analize_Distances.htm");
        }
    }
}
