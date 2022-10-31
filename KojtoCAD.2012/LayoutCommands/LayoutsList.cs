using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KojtoCAD.LayoutCommands
{
    public partial class LayoutsList : Form
    {
        private readonly string[] _layouts;

        public LayoutsList(string[] layouts)
        {
            InitializeComponent();
            this._layouts = layouts;
            this.Layouts.Items.AddRange(layouts);
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
