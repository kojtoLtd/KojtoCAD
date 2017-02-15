using System.Windows.Forms;

namespace KojtoCAD.Unfold._3d
{
    public partial class BAHelpWin : Form
    {
        public BAHelpWin()
        {
            InitializeComponent();
        }

        private void BAHelpWin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // The user pressed ESC - then close the form.
            if (e.KeyChar == (char)27)
            {
                Close();
            }
        }
    }
}