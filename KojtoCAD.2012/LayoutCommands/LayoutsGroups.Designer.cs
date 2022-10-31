namespace KojtoCAD.LayoutCommands
{
    partial class LayoutsGroups
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.GroupsTable = new System.Windows.Forms.DataGridView();
            this.Layouts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Dwg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Actions = new System.Windows.Forms.DataGridViewButtonColumn();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.AddGroupBtn = new System.Windows.Forms.Button();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SelectDirBtn = new System.Windows.Forms.Button();
            this.ResultsDirText = new System.Windows.Forms.TextBox();
            this.ErrorMessage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.GroupsTable)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupsTable
            // 
            this.GroupsTable.AllowUserToAddRows = false;
            this.GroupsTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupsTable.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GroupsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GroupsTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Layouts,
            this.Dwg,
            this.Actions});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GroupsTable.DefaultCellStyle = dataGridViewCellStyle1;
            this.GroupsTable.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.GroupsTable.Location = new System.Drawing.Point(12, 41);
            this.GroupsTable.Name = "GroupsTable";
            this.GroupsTable.RowHeadersVisible = false;
            this.GroupsTable.Size = new System.Drawing.Size(650, 261);
            this.GroupsTable.TabIndex = 1;
            this.GroupsTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GroupsTable_CellClick);
            // 
            // Layouts
            // 
            this.Layouts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Layouts.FillWeight = 497F;
            this.Layouts.HeaderText = "Layouts";
            this.Layouts.Name = "Layouts";
            this.Layouts.ReadOnly = true;
            this.Layouts.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Dwg
            // 
            this.Dwg.FillWeight = 150F;
            this.Dwg.HeaderText = "Dwg";
            this.Dwg.MaxInputLength = 127;
            this.Dwg.Name = "Dwg";
            this.Dwg.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Dwg.Width = 150;
            // 
            // Actions
            // 
            this.Actions.FillWeight = 30F;
            this.Actions.HeaderText = "";
            this.Actions.Name = "Actions";
            this.Actions.ReadOnly = true;
            this.Actions.Width = 30;
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.Location = new System.Drawing.Point(587, 331);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 3;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.Location = new System.Drawing.Point(506, 331);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // AddGroupBtn
            // 
            this.AddGroupBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddGroupBtn.Location = new System.Drawing.Point(12, 12);
            this.AddGroupBtn.Name = "AddGroupBtn";
            this.AddGroupBtn.Size = new System.Drawing.Size(94, 23);
            this.AddGroupBtn.TabIndex = 5;
            this.AddGroupBtn.Text = "Add new group";
            this.AddGroupBtn.UseVisualStyleBackColor = true;
            this.AddGroupBtn.Click += new System.EventHandler(this.AddGroupBtn_Click);
            // 
            // SelectDirBtn
            // 
            this.SelectDirBtn.Location = new System.Drawing.Point(189, 13);
            this.SelectDirBtn.Name = "SelectDirBtn";
            this.SelectDirBtn.Size = new System.Drawing.Size(111, 23);
            this.SelectDirBtn.TabIndex = 6;
            this.SelectDirBtn.Text = "Select results folder";
            this.SelectDirBtn.UseVisualStyleBackColor = true;
            this.SelectDirBtn.Click += new System.EventHandler(this.SelectDirBtn_Click);
            // 
            // ResultsDirText
            // 
            this.ResultsDirText.Enabled = false;
            this.ResultsDirText.Location = new System.Drawing.Point(306, 15);
            this.ResultsDirText.Name = "ResultsDirText";
            this.ResultsDirText.Size = new System.Drawing.Size(356, 20);
            this.ResultsDirText.TabIndex = 7;
            // 
            // ErrorMessage
            // 
            this.ErrorMessage.AutoSize = true;
            this.ErrorMessage.BackColor = System.Drawing.SystemColors.Control;
            this.ErrorMessage.ForeColor = System.Drawing.Color.Red;
            this.ErrorMessage.Location = new System.Drawing.Point(12, 305);
            this.ErrorMessage.Name = "ErrorMessage";
            this.ErrorMessage.Size = new System.Drawing.Size(58, 13);
            this.ErrorMessage.TabIndex = 8;
            this.ErrorMessage.Text = "E R R O R";
            // 
            // LayoutsGroups
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(674, 363);
            this.Controls.Add(this.ErrorMessage);
            this.Controls.Add(this.ResultsDirText);
            this.Controls.Add(this.SelectDirBtn);
            this.Controls.Add(this.AddGroupBtn);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.GroupsTable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LayoutsGroups";
            this.ShowIcon = false;
            this.Text = "Select layout groups";
            ((System.ComponentModel.ISupportInitialize)(this.GroupsTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView GroupsTable;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button AddGroupBtn;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.Button SelectDirBtn;
        private System.Windows.Forms.TextBox ResultsDirText;
        private System.Windows.Forms.DataGridViewTextBoxColumn Layouts;
        private System.Windows.Forms.DataGridViewTextBoxColumn Dwg;
        private System.Windows.Forms.DataGridViewButtonColumn Actions;
        private System.Windows.Forms.Label ErrorMessage;
    }
}