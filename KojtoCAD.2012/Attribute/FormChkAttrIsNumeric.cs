using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
using KojtoCAD.Properties;

namespace KojtoCAD.Attribute
{
    public partial class FormChkAttrIsNumeric : Form
    {
         private ArrayList BLOKS = new ArrayList();

         public FormChkAttrIsNumeric()
        {
            InitializeComponent();
        }

        public FormChkAttrIsNumeric(ref ArrayList Blocks, ref ArrayList BlocksWithLessThanThreeAttribute)
        {
           
            InitializeComponent();
            BLOKS = new ArrayList();
            foreach (MyBlock block in Blocks)
            {
                BLOKS.Add(block);
                comboBoxBlocks.Items.Add(block.Blockname);
            }
            foreach (MyBlock block in BlocksWithLessThanThreeAttribute)
            {
                BLOKS.Add(block);
                comboBoxBlocks.Items.Add(block.Blockname);
            }


            comboBoxExistAttribute.Items.Add("By Layer");
            comboBoxExistAttribute.Items.Add("By Block");
            comboBoxExistAttribute.Items.Add("Red");            
            comboBoxExistAttribute.Items.Add("Yellow");
            comboBoxExistAttribute.Items.Add("Green");
            comboBoxExistAttribute.Items.Add("Cyan");
            comboBoxExistAttribute.Items.Add("Blue");
            comboBoxExistAttribute.Items.Add("Magenta");
            comboBoxExistAttribute.Items.Add("Whaite");
            comboBoxExistAttribute.Items.Add("Gray");

            comboBoxNotExist.Items.Add("By Layer");
            comboBoxNotExist.Items.Add("By Block");
            comboBoxNotExist.Items.Add("Red");
            comboBoxNotExist.Items.Add("Yellow");
            comboBoxNotExist.Items.Add("Green");
            comboBoxNotExist.Items.Add("Cyan");
            comboBoxNotExist.Items.Add("Blue");
            comboBoxNotExist.Items.Add("Magenta");
            comboBoxNotExist.Items.Add("Whaite");
            comboBoxNotExist.Items.Add("Gray");

            comboBoxExistAttribute.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxNotExist.DrawMode = DrawMode.OwnerDrawFixed;

            textBoxExcelFile.Text = Settings.Default.attChecker_excelFile;
            textBoxOutFolder.Text = Settings.Default.attChecker_logFile;
            checkBoxAllFiles.Checked = Settings.Default.attChecker_traverseAllFiles;
            comboBoxSheets.Text = Settings.Default.attChecker_selectedSheet;
            comboBoxExistAttribute.Text = Settings.Default.attChecker_existingAttributeColor;
            comboBoxNotExist.Text = Settings.Default.attChecker_unExistingAttributeColor;
            comboBoxBlocks.Text = Settings.Default.attChecker_selecetdBlock;
            comboBoxAttributes.Text = Settings.Default.attChecker_selectedAttribute;
            checkBoxRealNumber.Checked = Settings.Default.attChecker_isRealNumber;

            checkBoxRealNumber.Checked = true;

            radioButtonDP.Enabled = true;
            radioButtonDC.Enabled = true;

            radioButtonDP.Checked = Settings.Default.attChecker_hasPointDelimiter;
            radioButtonDC.Checked = !Settings.Default.attChecker_hasCommaDelimiter;

            checkBoxSearchInXLSX.Checked = true;

            textBoxExcelFile.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxSheets.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxExistAttribute.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxNotExist.Enabled = checkBoxSearchInXLSX.Checked;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if ((textBoxExcelFile.Text.Length < 4) && (checkBoxSearchInXLSX.Checked))
            {
                MessageBox.Show("Excel file to search attribute content ?", "E R R O R");
                textBoxExcelFile.Focus();
                return;
            }
            if (textBoxOutFolder.Text.Length < 2)
            {
                MessageBox.Show("Folder for Log File ?", "E R R O R");
                textBoxOutFolder.Focus();
                return;
            }

            if (comboBoxBlocks.Text.Length < 1)
            {
                MessageBox.Show("Select Block ?", "E R R O R");
                comboBoxBlocks.Focus();
                return;
            }

            if (comboBoxAttributes.Text.Length < 1)
            {
                MessageBox.Show("Select Tag ?", "E R R O R");
                comboBoxAttributes.Focus();
                return;
            }

            if ((comboBoxExistAttribute.Text.Length < 1) && (checkBoxSearchInXLSX.Checked))
            {
                MessageBox.Show("Attribute colour if it's content is found in the Excel File ?", "E R R O R");
                comboBoxExistAttribute.Focus();
                return;
            }

            if( (comboBoxNotExist.Text.Length < 1) && (checkBoxSearchInXLSX.Checked))
            {
                MessageBox.Show("Attribute colour if it's content is NOT found in the Excel File ?", "E R R O R");
                comboBoxNotExist.Focus();
                return;
            }

            if ((comboBoxSheets.Text.Length < 1) && (checkBoxSearchInXLSX.Checked))
            {
                MessageBox.Show("Select sheet of the *.XLSX file ?", "E R R O R");
                comboBoxSheets.Focus();
                return;
            }

            textBoxExcelFile.Text = Settings.Default.attChecker_excelFile;
            textBoxOutFolder.Text = Settings.Default.attChecker_logFile;
            checkBoxAllFiles.Checked = Settings.Default.attChecker_traverseAllFiles;
            comboBoxSheets.Text = Settings.Default.attChecker_selectedSheet;
            comboBoxExistAttribute.Text = Settings.Default.attChecker_existingAttributeColor;
            comboBoxNotExist.Text = Settings.Default.attChecker_unExistingAttributeColor;
            comboBoxBlocks.Text = Settings.Default.attChecker_selecetdBlock;
            comboBoxAttributes.Text = Settings.Default.attChecker_selectedAttribute;
            checkBoxRealNumber.Checked = Settings.Default.attChecker_isRealNumber;

            DialogResult = DialogResult.OK;
        }

        public string GetExelFileName()
        {
            return textBoxExcelFile.Text;
        }
        public string OutFolder()
        {
            return textBoxOutFolder.Text;
        }
        public string SheetName()
        {
            return comboBoxSheets.Text;
        }

        public int ExistAttributesColorIndex()
        {
            switch (comboBoxExistAttribute.SelectedIndex)
            {
                case 0: return 256;
                case 1: return 0;
                case 2: return 1;
                case 3: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                case 7: return 6;
                case 8: return 7;
            }

            return 256;
        }
        public int NotExistAttributesColorIndex()
        {
            switch (comboBoxNotExist.SelectedIndex)
            {
                case 0: return 256;
                case 1: return 0;
                case 2: return 1;
                case 3: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                case 7: return 6;
                case 8: return 7;
            }
           
            return 256;
        }

        public string ExistAttributesColorText()
        {
            return comboBoxExistAttribute.Text;
        }
        public string NotExistAttributesColorText()
        {
            return comboBoxNotExist.Text;
        }

        public string BlocName()
        {
            return comboBoxBlocks.Text;
        }
        public string AttributeTag()
        {
            return comboBoxAttributes.Text;
        }
        public bool CheckBoxAllFiles()
        {
            return checkBoxAllFiles.Checked;
        }
        public bool RadioButtonDP()
        {
            return radioButtonDP.Checked;
        }
        public bool CheckRealNumber()
        {
            return checkBoxRealNumber.Checked;
        }
        public bool SearchInXLSX()
        {
            return checkBoxSearchInXLSX.Checked;
        }


        private void checkBoxRealNumber_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRealNumber.Checked)
            {
                radioButtonDP.Enabled = true;
                radioButtonDC.Enabled = true;
            }
            else
            {
                radioButtonDP.Enabled = false;
                radioButtonDC.Enabled = false;
            }
        }

        private void buttonExcelFileBrowse_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxExcelFile.Text = dlg.FileName;
                StringCollection coll = AttributeHelper.GetSheetInfo(dlg.FileName);
                comboBoxSheets.Items.Clear();
                foreach (string str in coll)
                {
                    comboBoxSheets.Items.Add(str);
                }
            }
        }

        private void buttonOutFolderBrowse_Click_1(object sender, EventArgs e)
        {
            textBoxOutFolder.Text = AttributeHelper.GetFolder();
        }

        private void buttonCancel_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBoxBlocks_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            comboBoxAttributes.Items.Clear();
            foreach (MyBlock block in BLOKS)
            {

                if (block.Blockname == comboBoxBlocks.Text)
                {
                    foreach (string str in block.Attributes)
                    {
                        comboBoxAttributes.Items.Add(str);
                    }
                    break;
                }
            }
        }

        private void comboBoxExistAttribute_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 70, rect.Y + 5,
                                rect.Width - 50, rect.Height - 3);
            }
        }

        private void comboBoxNotExist_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 70, rect.Y + 5,
                                rect.Width - 50, rect.Height - 3);
            }
        }

        private void checkBoxSearchInXLSX_CheckedChanged(object sender, EventArgs e)
        {
            textBoxExcelFile.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxSheets.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxExistAttribute.Enabled = checkBoxSearchInXLSX.Checked;
            comboBoxNotExist.Enabled = checkBoxSearchInXLSX.Checked;
        }
    }
}
