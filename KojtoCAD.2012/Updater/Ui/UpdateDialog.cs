using System;
using System.Windows.Forms;

namespace KojtoCAD.Updater.Ui
{
    public partial class UpdateDialog : Form
    {
        private readonly string _currentVersion;
        private readonly string _newVersion;

        public UpdateDialog(string currentVersion, string newVersion)
        {
            _currentVersion = currentVersion;
            _newVersion = newVersion;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnFormLoading(object sender, EventArgs e)
        {
            label5.Text = _currentVersion;
            label6.Text = _newVersion;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
