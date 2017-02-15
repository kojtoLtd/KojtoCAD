using System;
using System.Windows.Forms;

namespace KojtoCAD.PointCloud
{
    public partial class LoadPointCloudForm : Form
    {
        public int minX; public int maxX;
        public int minY; public int maxY;
        public int minZ; public int maxZ;
        public string m_SS = ""; public int m_PN;
        public string FN1 = "";// = new string();
        public string FN2 = "";// = new string();
        public string FN3 = "";// = new string();
        public string FN4 = "";// = new string();
        public string FN5 = "";// = new string();

        public LoadPointCloudForm()
        {
            InitializeComponent();
            SS.Text = ";";
            PN.Text = "100";
            Filename1.Text = "";
            Filename2.Text = "";
            Filename3.Text = "";
            Filename4.Text = "";
            Filename5.Text = "";
            X1.Text = "0"; X2.Text = "0";
            Y1.Text = "0"; Y2.Text = "0";
            Z1.Text = "0"; Z2.Text = "0";
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                FN1 = Filename1.Text; FN2 = Filename2.Text; FN3 = Filename3.Text; FN4 = Filename4.Text; FN5 = Filename5.Text;
                if ((FN1.Length < 2) && (FN2.Length < 2) && (FN3.Length < 2) && (FN4.Length < 2) && (FN5.Length < 2)) { throw new FormatException(); }
                m_SS = SS.Text; if ((m_SS.Length < 1) || (m_SS.Length > 1)) { throw new FormatException(); }
                m_PN = int.Parse(PN.Text); if (m_PN <= 0) { throw new FormatException(); }
                if (X1.Text.Length < 1) { throw new FormatException(); }
                if (X2.Text.Length < 1) { throw new FormatException(); }
                if (Y1.Text.Length < 1) { throw new FormatException(); }
                if (Y2.Text.Length < 1) { throw new FormatException(); }
                if (Z1.Text.Length < 1) { throw new FormatException(); }
                if (Z2.Text.Length < 1) { throw new FormatException(); }
                minX = int.Parse(X1.Text);
                maxX = int.Parse(X2.Text);
                minY = int.Parse(Y1.Text);
                maxY = int.Parse(Y2.Text);
                minZ = int.Parse(Z1.Text);
                maxZ = int.Parse(Z2.Text);

                if (minX > maxX) { int buff = minX; minX = maxX; maxX = buff; }
                if (minY > maxY) { int buff = minY; minY = maxY; maxY = buff; }
                if (minZ > maxZ) { int buff = minZ; minZ = maxZ; maxZ = buff; }

                DialogResult = DialogResult.OK;
            }
            catch (FormatException)
            {
                if (FN1.Length < 2)
                {
                    MessageBox.Show("FileName1 = ?");
                    Filename1.Text = "";
                    Filename1.Focus();
                }
                if ((m_SS.Length < 1) || (m_SS.Length > 1))
                {
                    MessageBox.Show("Separation Character = ?");
                    SS.Text = "";
                    SS.Focus();
                }
                if (m_PN <= 0)
                {
                    MessageBox.Show("Draw a Point for Every = ?");
                    PN.Text = "";
                    PN.Focus();
                }
                if (X1.Text.Length < 1)
                {
                    MessageBox.Show("Limit1 - X = ?");
                    X1.Text = "";
                    X1.Focus();
                }
                if (X2.Text.Length < 1)
                {
                    MessageBox.Show("Limit2 - X = ?");
                    X2.Text = "";
                    X2.Focus();
                }
                if (Y1.Text.Length < 1)
                {
                    MessageBox.Show("Limit1 - Y = ?");
                    Y1.Text = "";
                    Y1.Focus();
                }
                if (Y2.Text.Length < 1)
                {
                    MessageBox.Show("Limit2 - Y = ?");
                    Y2.Text = "";
                    Y2.Focus();
                }
                if (Z1.Text.Length < 1)
                {
                    MessageBox.Show("Limit1 - Z = ?");
                    Z1.Text = "";
                    Z1.Focus();
                }
                if (Z2.Text.Length < 1)
                {
                    MessageBox.Show("Limit2 - Z = ?");
                    Z2.Text = "";
                    Z2.Focus();
                }
            }
        }

        private void button_browse1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Filename1.Text = dlg.FileName;
            }
        }

        private void button_browse2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Filename2.Text = dlg.FileName;
            }
        }

        private void button_browse3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Filename3.Text = dlg.FileName;
            }
        }

        private void button_browse4_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Filename4.Text = dlg.FileName;
            }
        }

        private void button_browse5_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Filename5.Text = dlg.FileName;
            }
        }

        private void LoadPointCloudForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }


    }
}

