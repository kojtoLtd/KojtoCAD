using System;
using System.Threading;
using System.Windows.Forms;

namespace KojtoCAD.Updater.Ui
{
    public partial class UpdateProgressDialog : Form
    {
        private readonly Progress<UpdateProgressData> _progress;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public UpdateProgressDialog()
        {
            _progress = new Progress<UpdateProgressData>();
            _progress.ProgressChanged += (s, e) =>
            {
                if (CancellationTokenSource == null || CancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                // 5% for autoloader 
                if (e.EditAutoloaderSettingsCompleted)
                {
                    progressBar.Value = 100;
                    btnDone.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }

                // 5% for leftovers and 5% for copy 
                if (e.RemoveTemp || e.CopyCompleted)
                {
                    progressBar.Value += 5;
                    return;
                }

                // 75% for download
                var totalProgress = (int)Math.Round((double)e.CurrentFile / e.FilesCount * 75);
                progressBar.Value = totalProgress;
            };

            InitializeComponent();

            btnDone.Enabled = false;
            btnCancel.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
            statusText.Text = "Cancelling...";
            Enabled = false;
            // close will be called after cleaning-up
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        public IProgress<UpdateProgressData> Progress
        {
            get { return _progress; }
        }

        public CancellationTokenSource CancellationTokenSource
        {
            get { return _cancellationTokenSource; }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource.Dispose();
        }
    }
}
