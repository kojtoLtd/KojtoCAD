using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace KojtoCAD.LayoutCommands
{
    public partial class LayoutsGroups : Form
    {
        LayoutsList _layoutsListForm;
        private readonly List<string> _layouts;
        public string ResultsPath;
        public ICollection<SplitFileResult> ResultFiles { get; } = new List<SplitFileResult>();

        public LayoutsGroups(string[] layouts)
        {
            InitializeComponent();

            _layoutsListForm = new LayoutsList(layouts);
            _layouts = layouts.ToList();

            ErrorMessage.Hide();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            var error = false;
            ResultFiles.Clear();

            // Gather all required data
            ResultsPath = FolderBrowserDialog.SelectedPath;
            if (string.IsNullOrEmpty(ResultsPath))
            {
                error = true;
            }

            foreach (DataGridViewRow row in GroupsTable.Rows)
            {
                // Validation: no missing result file names
                var fileName = (string)row.Cells[1].Value;
                if (string.IsNullOrEmpty(fileName))
                {
                    error = true;
                }

                // Validation: no empty layout groups
                var text = (string)row.Cells[0].Value;
                var selectedLayouts = text.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (selectedLayouts.Length == 0)
                {
                    error = true;
                }

                // Validation: no duplicate result file names
                if (ResultFiles.Any(x=>x.FileName == fileName))
                {
                    error = true;
                }

                ResultFiles.Add(new(fileName, selectedLayouts));
            }

            if (error)
            {
                ErrorMessage.Text = "Make sure the output directory is selected and unique file names are provided for each group. Empty layout groups are not allowed.";
                ErrorMessage.Show();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SelectDirBtn_Click(object sender, EventArgs e)
        {
            var res = FolderBrowserDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                ResultsDirText.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void AddGroupBtn_Click(object sender, EventArgs e)
        {
            // Nothing preselected
            _layoutsListForm.Layouts.ClearSelected();

            var result = _layoutsListForm.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            var selectedLayous = _layoutsListForm.Layouts.SelectedItems
                .Cast<string>()
                .OrderBy(x => x)
                .ToArray();
            
            GroupsTable.Rows.Add(
                string.Join("; ", selectedLayous),
                selectedLayous.Length != 0 ? selectedLayous[0] : string.Empty,
                "X");// X - the delete button in the last column
        }

        private void GroupsTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == GroupsTable.ColumnCount - 1)
            {
                // Delete row
                GroupsTable.Rows.RemoveAt(e.RowIndex);
                return;
            }

            if (e.ColumnIndex != 0)
            {
                // Don't care
                return;
            }

            // Get data from selected row
            var layoutsInGroupText = GroupsTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
            if (layoutsInGroupText != null)
            {
                // Clicked on fulfilled group: edit
                var selectedLayouts = layoutsInGroupText.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var layout in selectedLayouts)
                {
                    _layoutsListForm.Layouts.SetSelected(_layouts.IndexOf(layout), true);
                }
            }
            else
            {
                // Clicked on an empty row: show not selected list
                _layoutsListForm.Layouts.ClearSelected();
            }

            var result = _layoutsListForm.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            // Get selected data and put it into the table
            var selectedLayous = _layoutsListForm.Layouts.SelectedItems.Cast<string>().ToArray();
            GroupsTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = string.Join("; ", selectedLayous);
        }
    }
}
