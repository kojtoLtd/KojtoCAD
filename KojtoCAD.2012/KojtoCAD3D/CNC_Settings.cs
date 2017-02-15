using System;
using System.Windows.Forms;

namespace KojtoCAD.KojtoCAD3D
{
    public partial class CNC_Settings : Form
    {
        private double L_;
        public double L { get { return (L_); } }

        private double Lp_;
        public double Lp { get { return (Lp_); } }

        private double Ls_;
        public double Ls { get { return (Ls_); } }

        private double R_;
        public double R { get { return (R_); } }

        private double toolR_;
        public double toolR { get { return (toolR_); } }

        private double wsHeight_;
        public double Workpiece_Height { get { return (wsHeight_); } }

        private double wsLength_;
        public double Workpiece_Length { get { return (wsLength_); } }

        private double wsWidth_;
        public double Workpiece_Width { get { return (wsWidth_); } }

        public CNC_Settings()
        {
            L_ = 0.0; Lp_ = 90.0; Ls_ = 75.0; R_ = 65.0; toolR_ = 10.0;
            wsHeight_ = 80.0;

            InitializeComponent();
            textBox_L.Text = "0.0";
            textBox_Lp.Text = "90.0";
            textBox_Ls.Text = "75.0";
            textBox_R.Text = "65.0";
            textBox_Tool_R.Text = "10.0";
            textBox_Workpiece_Height.Text = "80.0";
            textBox_Workpiece_Length.Text = "175.0";
            textBox_Workpiece_Width.Text = "175.0";
            textBox_L.Focus();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                L_ = double.Parse(textBox_L.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect L !", "E R R O R");
                textBox_L.Focus();
                return;
            }

            try
            {
                Lp_ = double.Parse(textBox_Lp.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect L prim  !", "E R R O R");
                textBox_Lp.Focus();
                return;
            }

            try
            {
                Ls_ = double.Parse(textBox_Ls.Text);
            }
            catch
            {
                MessageBox.Show("Missing Corect L second  !", "E R R O R");
                textBox_Ls.Focus();
                return;
            }

            try
            {
                R_ = double.Parse(textBox_R.Text);
                if (R_ <= 0.0)
                {
                    MessageBox.Show("R should have a positive value !", "E R R O R");
                    textBox_R.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect R  !", "E R R O R");
                textBox_R.Focus();
                return;
            }

            try
            {
                toolR_ = double.Parse(textBox_Tool_R.Text);
                if (toolR_ <= 0.0)
                {
                    MessageBox.Show("Tool R should have a positive value !", "E R R O R");
                    textBox_Tool_R.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Tool R  !", "E R R O R");
                textBox_Tool_R.Focus();
                return;
            }

            try
            {
                wsHeight_ = double.Parse(textBox_Workpiece_Height.Text);
                if (wsHeight_ <= 0.0)
                {
                    MessageBox.Show("Workpiece Height should have a positive value !", "E R R O R");
                    textBox_Workpiece_Height.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Workpiece Height !", "E R R O R");
                textBox_Workpiece_Height.Focus();
                return;
            }

            try
            {
                wsLength_ = double.Parse(textBox_Workpiece_Length.Text);
                if (wsLength_ <= 0.0)
                {
                    MessageBox.Show("Workpiece Length should have a positive value !", "E R R O R");
                    textBox_Workpiece_Length.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Workpiece Length !", "E R R O R");
                textBox_Workpiece_Length.Focus();
                return;
            }

            try
            {
                wsWidth_ = double.Parse(textBox_Workpiece_Width.Text);
                if (wsWidth_ <= 0.0)
                {
                    MessageBox.Show("Workpiece Width should have a positive value !", "E R R O R");
                    textBox_Workpiece_Width.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Workpiece Width !", "E R R O R");
                textBox_Workpiece_Width.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
