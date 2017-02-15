namespace KojtoCAD.Plotter
{
    partial class PublishStatusDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileStatusList = new System.Windows.Forms.ListView();
            this.FileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PublishedPathDWF = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PublishPathPDF = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timePublish = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.StopBlk = new System.Windows.Forms.Button();
            this.doneBlk = new System.Windows.Forms.Button();
            this.publishProBar = new System.Windows.Forms.ProgressBar();
            this.label_seconds = new System.Windows.Forms.Label();
            this.textSeconds = new System.Windows.Forms.TextBox();
            this.retry_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fileStatusList
            // 
            this.fileStatusList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileStatusList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileName,
            this.Status,
            this.PublishedPathDWF,
            this.PublishPathPDF,
            this.timePublish});
            this.fileStatusList.FullRowSelect = true;
            this.fileStatusList.GridLines = true;
            this.fileStatusList.Location = new System.Drawing.Point(12, 54);
            this.fileStatusList.Name = "fileStatusList";
            this.fileStatusList.Size = new System.Drawing.Size(787, 223);
            this.fileStatusList.TabIndex = 2;
            this.fileStatusList.UseCompatibleStateImageBehavior = false;
            this.fileStatusList.View = System.Windows.Forms.View.Details;
            // 
            // FileName
            // 
            this.FileName.Text = "File name";
            this.FileName.Width = 214;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 102;
            // 
            // PublishedPathDWF
            // 
            this.PublishedPathDWF.Text = "DWF Published path";
            this.PublishedPathDWF.Width = 131;
            // 
            // PublishPathPDF
            // 
            this.PublishPathPDF.Text = "PDF published path";
            this.PublishPathPDF.Width = 116;
            // 
            // timePublish
            // 
            this.timePublish.Text = "Time";
            this.timePublish.Width = 78;
            // 
            // StopBlk
            // 
            this.StopBlk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StopBlk.Location = new System.Drawing.Point(621, 296);
            this.StopBlk.Name = "StopBlk";
            this.StopBlk.Size = new System.Drawing.Size(76, 28);
            this.StopBlk.TabIndex = 0;
            this.StopBlk.Text = "Stop";
            this.StopBlk.UseVisualStyleBackColor = true;
            this.StopBlk.Click += new System.EventHandler(this.StopBlk_Click);
            // 
            // doneBlk
            // 
            this.doneBlk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.doneBlk.Location = new System.Drawing.Point(723, 296);
            this.doneBlk.Name = "doneBlk";
            this.doneBlk.Size = new System.Drawing.Size(76, 28);
            this.doneBlk.TabIndex = 1;
            this.doneBlk.Text = "Done";
            this.doneBlk.UseVisualStyleBackColor = true;
            this.doneBlk.Click += new System.EventHandler(this.DoneBlk_Click);
            // 
            // publishProBar
            // 
            this.publishProBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.publishProBar.Location = new System.Drawing.Point(16, 298);
            this.publishProBar.Name = "publishProBar";
            this.publishProBar.Size = new System.Drawing.Size(460, 25);
            this.publishProBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.publishProBar.TabIndex = 3;
            // 
            // label_seconds
            // 
            this.label_seconds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_seconds.AutoSize = true;
            this.label_seconds.Location = new System.Drawing.Point(497, 20);
            this.label_seconds.Name = "label_seconds";
            this.label_seconds.Size = new System.Drawing.Size(226, 13);
            this.label_seconds.TabIndex = 10;
            this.label_seconds.Text = "Publish time out per drawing in seconds ( > 10)";
            this.label_seconds.Visible = false;
            // 
            // textSeconds
            // 
            this.textSeconds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textSeconds.Location = new System.Drawing.Point(746, 17);
            this.textSeconds.Name = "textSeconds";
            this.textSeconds.Size = new System.Drawing.Size(49, 20);
            this.textSeconds.TabIndex = 9;
            this.textSeconds.Text = "300";
            this.textSeconds.Visible = false;
            // 
            // retry_button
            // 
            this.retry_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.retry_button.Location = new System.Drawing.Point(521, 296);
            this.retry_button.Name = "retry_button";
            this.retry_button.Size = new System.Drawing.Size(76, 28);
            this.retry_button.TabIndex = 4;
            this.retry_button.Text = "Retry";
            this.retry_button.UseVisualStyleBackColor = true;
            this.retry_button.Visible = false;
            // 
            // PublishStatusDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 334);
            this.Controls.Add(this.label_seconds);
            this.Controls.Add(this.textSeconds);
            this.Controls.Add(this.retry_button);
            this.Controls.Add(this.publishProBar);
            this.Controls.Add(this.doneBlk);
            this.Controls.Add(this.StopBlk);
            this.Controls.Add(this.fileStatusList);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PublishStatusDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Publish Status";
            this.Load += new System.EventHandler(this.PublishStatusDlg_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PublishStatusDlg_KeyPress);
            this.ResumeLayout(false);
            this.PerformLayout();

}

        #endregion

        private System.Windows.Forms.ListView fileStatusList;
        private System.Windows.Forms.ColumnHeader FileName;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.ColumnHeader PublishedPathDWF;
        private System.Windows.Forms.Button StopBlk;
        private System.Windows.Forms.Button doneBlk;
        private System.Windows.Forms.ColumnHeader PublishPathPDF;
        private System.Windows.Forms.ColumnHeader timePublish;
      private System.Windows.Forms.ProgressBar publishProBar;
        private System.Windows.Forms.Label label_seconds;
        private System.Windows.Forms.TextBox textSeconds;
      private System.Windows.Forms.Button retry_button;
    }
}