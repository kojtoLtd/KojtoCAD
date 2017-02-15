namespace KojtoCAD.BlockItems.ProfileSearchEngine
{
  /// <summary>
  /// Dialog in which the user chooses a block to import
  /// </summary>
  partial class ProfileSearchEngine
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
            this.components = new System.ComponentModel.Container();
            this.comboBoxManufacturers = new System.Windows.Forms.ComboBox();
            this.listBoxProfiles = new System.Windows.Forms.ListBox();
            this.textBoxSearchPattern = new System.Windows.Forms.TextBox();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonInsert = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.buttonSearch = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonUpdateProfiles = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.buttonProfilesDirectory = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxManufacturers
            // 
            this.comboBoxManufacturers.FormattingEnabled = true;
            this.comboBoxManufacturers.ItemHeight = 13;
            this.comboBoxManufacturers.Location = new System.Drawing.Point(12, 27);
            this.comboBoxManufacturers.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBoxManufacturers.MaxDropDownItems = 12;
            this.comboBoxManufacturers.Name = "comboBoxManufacturers";
            this.comboBoxManufacturers.Size = new System.Drawing.Size(251, 21);
            this.comboBoxManufacturers.TabIndex = 0;
            this.comboBoxManufacturers.SelectedIndexChanged += new System.EventHandler(this.comboBoxManufacturers_SelectedIndexChanged);
            // 
            // listBoxProfiles
            // 
            this.listBoxProfiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBoxProfiles.FormattingEnabled = true;
            this.listBoxProfiles.ItemHeight = 12;
            this.listBoxProfiles.Location = new System.Drawing.Point(267, 27);
            this.listBoxProfiles.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.listBoxProfiles.Name = "listBoxProfiles";
            this.listBoxProfiles.Size = new System.Drawing.Size(939, 316);
            this.listBoxProfiles.TabIndex = 1;
            this.listBoxProfiles.TabStop = false;
            this.listBoxProfiles.SelectedIndexChanged += new System.EventHandler(this.listBoxProfiles_SelectedIndexChanged);
            this.listBoxProfiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxProfiles_DoubleClick);
            // 
            // textBoxSearchPattern
            // 
            this.textBoxSearchPattern.Location = new System.Drawing.Point(12, 75);
            this.textBoxSearchPattern.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBoxSearchPattern.Name = "textBoxSearchPattern";
            this.textBoxSearchPattern.Size = new System.Drawing.Size(130, 20);
            this.textBoxSearchPattern.TabIndex = 2;
            this.textBoxSearchPattern.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxSearchPattern_KeyPress);
            // 
            // buttonExit
            // 
            this.buttonExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonExit.Location = new System.Drawing.Point(146, 101);
            this.buttonExit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(117, 25);
            this.buttonExit.TabIndex = 7;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonInsert
            // 
            this.buttonInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonInsert.Location = new System.Drawing.Point(12, 101);
            this.buttonInsert.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(130, 25);
            this.buttonInsert.TabIndex = 5;
            this.buttonInsert.Text = "Insert";
            this.buttonInsert.UseVisualStyleBackColor = true;
            this.buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonSearch.Location = new System.Drawing.Point(146, 59);
            this.buttonSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(117, 36);
            this.buttonSearch.TabIndex = 4;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(11, 130);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(252, 148);
            this.pictureBox.TabIndex = 8;
            this.pictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Choose Manufacturer";
           
            // 
            // buttonUpdateProfiles
            // 
            this.buttonUpdateProfiles.Location = new System.Drawing.Point(146, 284);
            this.buttonUpdateProfiles.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonUpdateProfiles.Name = "buttonUpdateProfiles";
            this.buttonUpdateProfiles.Size = new System.Drawing.Size(117, 44);
            this.buttonUpdateProfiles.TabIndex = 11;
            this.buttonUpdateProfiles.Text = "Update profiles";
            this.buttonUpdateProfiles.UseVisualStyleBackColor = true;
            this.buttonUpdateProfiles.Click += new System.EventHandler(this.buttonUpdateProfiles_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(11, 330);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(252, 13);
            this.progressBar.TabIndex = 12;
            this.progressBar.Visible = false;
            // 
            // buttonProfilesDirectory
            // 
            this.buttonProfilesDirectory.Location = new System.Drawing.Point(12, 284);
            this.buttonProfilesDirectory.Margin = new System.Windows.Forms.Padding(1);
            this.buttonProfilesDirectory.Name = "buttonProfilesDirectory";
            this.buttonProfilesDirectory.Size = new System.Drawing.Size(131, 44);
            this.buttonProfilesDirectory.TabIndex = 13;
            this.buttonProfilesDirectory.Text = "Browse For Profiles Directory";
            this.buttonProfilesDirectory.UseVisualStyleBackColor = true;
            this.buttonProfilesDirectory.Click += new System.EventHandler(this.buttonProfilesDirectory_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 59);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Enter Keywords to Search";
            
            // 
            // ProfileSearchEngine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1209, 355);
            this.Controls.Add(this.buttonProfilesDirectory);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonUpdateProfiles);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.buttonInsert);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.textBoxSearchPattern);
            this.Controls.Add(this.listBoxProfiles);
            this.Controls.Add(this.comboBoxManufacturers);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "ProfileSearchEngine";
            this.Text = "Import Block";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportBlockForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportBlockForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxManufacturers;
    private System.Windows.Forms.ListBox listBoxProfiles;
    private System.Windows.Forms.TextBox textBoxSearchPattern;
    private System.Windows.Forms.Button buttonExit;
    private System.Windows.Forms.Button buttonInsert;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.Button buttonSearch;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonUpdateProfiles;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Button buttonProfilesDirectory;
    private System.Windows.Forms.Label label3;
  }
}