using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KojtoCAD.Plotter
{
  public partial class PublishStatusDlg : Form
  {
    public PublishStatusDlg()
    {
      InitializeComponent();
    }
    
    private bool _stop;
    public bool Stop
    {
      set { _stop = value; }
      get { return _stop; }
    }

      private void PublishStatusDlg_Load(object sender, EventArgs e)
    {
      StopBlk.Enabled = true;
      retry_button.Enabled = false;
      textSeconds.Enabled = false;
      doneBlk.Enabled = false;
    }

    private void StopBlk_Click(object sender, EventArgs e)
    {
      _stop = true;
      StopBlk.Enabled = false;
      publishProBar.Visible = false;
      doneBlk.Enabled = true;
    }

    private void DoneBlk_Click(object sender, EventArgs e)
    {
      this.Close();
      XmlUtils.DeleteXml();
      XmlUtils.DeleteCopy();
    }

    public void PopulateList(ref List<PublishInfo> publishInfos, string timeout)
    {
      foreach (PublishInfo info in publishInfos)
      {
        ListViewItem item = fileStatusList.Items.Add(info.DwgName);
        info.Index = item.Index;
        item.SubItems.Add("");
        item.SubItems.Add("");
        item.SubItems.Add("");
        item.SubItems.Add("");
      }

      publishProBar.Maximum = publishInfos.Count;
      publishProBar.Step = 1;
      publishProBar.Minimum = 0;

      textSeconds.Text = timeout;
    }

    public void UpdateFileStatus(int idx, string status, string path, bool dwf, string dateTime)
    {
      fileStatusList.Items[idx].SubItems[1].Text = status;
      fileStatusList.Items[idx].SubItems[dwf ? 2 : 3].Text = path;
      fileStatusList.Items[idx].SubItems[4].Text = dateTime;
      fileStatusList.Refresh();
    }

    public void PublishOver(int failed)
    {
      StopBlk.Enabled = false;
      publishProBar.Visible = false;
      doneBlk.Enabled = true;

      if (failed != 0)
      {
        retry_button.Enabled = true;
        textSeconds.Enabled = true;
      }
    }

    public void Clear()
    {
      this.StopBlk.Enabled = true;
      retry_button.Enabled = false;
      textSeconds.Enabled = false;
      fileStatusList.Items.Clear();
      publishProBar.Visible = true;
    }

    public void newFile()
    {
      publishProBar.PerformStep();
    }

    private void PublishStatusDlg_KeyPress(object sender, KeyPressEventArgs e)
    {
        // The user pressed ESC - then close the form.
        if (e.KeyChar == (char)27)
        {
            Close();
        }
    }
   
  }
}