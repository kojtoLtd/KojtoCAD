using System;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace KojtoCAD.Ui.Commercial
{
    public partial class PrototypBanner : Form
    {
        private Timer _timeoutTimer;

        public PrototypBanner(int millisecondsTimeOut)
        {
            InitializeComponent();
            _timeoutTimer = new Timer(OnTimerElapsed, null, millisecondsTimeOut, System.Threading.Timeout.Infinite);
        }

        private void PrototypBanner_Load(object sender, EventArgs e)
        {
        }

        void OnTimerElapsed(object state)
        {
            _timeoutTimer.Dispose();
            DialogResult = DialogResult.OK;
        }

        private void PrototypBanner_FormClosing(object sender, FormClosingEventArgs e)
        {
 
        }
    }
}
