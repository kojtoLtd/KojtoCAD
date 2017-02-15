using System.Windows.Forms;

namespace KojtoCAD.Unfold._3d
{
    public partial class BDHelpWin : Form
    {
        public BDHelpWin()
        {
            InitializeComponent();
        }

        private void BDHelpWin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }
    }
}