using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;

namespace KojtoCAD.BlockItems.ProfileSearchEngine
{
    public partial class ProfileSearchEngine : Form
    {
        private const string ProfilesCatalogTitle = "Profiles";
        private const string ManufacturersCatalogTitle = "Manufacturers";

        private string _profilesCatalog;
        private string _manufacturersCatalog;
        private string _kojtoCadTempDir;

        private DataTable _profileDirsTable;

        private ILogger _logger;

        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        /// <summary>
        /// Check if any DWG from the dialog list is selected for import
        /// </summary>
        public bool DwgIsSelected { get; set; }

        /// <summary>
        /// The full patch plus name for the selected drawing
        /// </summary>
        public string DwgFileFullPath
        {
            get
            {
                if (!Settings.Default.FileServer.EndsWith("\\"))
                {
                    Settings.Default.FileServer = Settings.Default.FileServer + "\\";
                }
                return Settings.Default.FileServer + listBoxProfiles.Text;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileSearchEngine()
        {
            InitializeComponent();

            comboBoxManufacturers.Sorted = true;
        }

        private void ImportBlockForm_Load(object sender, EventArgs e)
        {
            _kojtoCadTempDir = Path.Combine(Path.GetTempPath(), Settings.Default.appDir);
            if (!Directory.Exists(_kojtoCadTempDir))
            {
                try
                {
                    Directory.CreateDirectory(_kojtoCadTempDir);
                }
                catch (Exception exception)
                {
                    _logger.Error("Can not create KojtoCAD temp dir.", exception);
                    errorProvider.SetError(buttonProfilesDirectory, "Can not create KojtoCAD temp dir. Contact support.");
                    return;
                }

            }
            _profilesCatalog = Path.Combine(_kojtoCadTempDir, "Profiles.xml");
            _manufacturersCatalog = Path.Combine(_kojtoCadTempDir, "Manufacturers.xml");

            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            try
            {
                var loadManufacturersResult = LoadManufacturers();
                if (!loadManufacturersResult)
                {
                    return;
                }
                this.textBoxSearchPattern.Text = Settings.Default.lastSelectedSearchPattern;
                this.comboBoxManufacturers.Text = Settings.Default.lastSelectedManufacturer;
            }
            catch (Exception exception)
            {
                _logger.Warn(exception.Message);
                //errorProvider.SetError(comboBoxManufacturers, "Set profiles directory and click update profiles.");
                //errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                //errorProvider.SetError(buttonUpdateProfiles, "Set profiles directory and click update profiles.");
            }
        }

        private bool LoadManufacturers()
        {
            try
            {
                if (!File.Exists(_manufacturersCatalog))
                {
                    errorProvider.SetError(buttonProfilesDirectory, "Did you set the profile directory?");
                    return false;
                }
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(_manufacturersCatalog);
                if (dataSet.Tables.Count == 0)
                {
                    errorProvider.SetError(comboBoxManufacturers, "Did you set the profile directory?");
                    errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                    return false;
                }
                this._profileDirsTable = dataSet.Tables[0];
                this._profileDirsTable.Columns[0].ColumnName = "ProfileDir";

                DataRow firstRow = this._profileDirsTable.NewRow();
                firstRow["ProfileDir"] = "All";
                this._profileDirsTable.Rows.Add(firstRow);

                this.comboBoxManufacturers.DataSource = this._profileDirsTable;
                this.comboBoxManufacturers.DisplayMember = "ProfileDir";
                return true;
            }
            catch (Exception exception)
            {
                _logger.Warn("Failed to populate Manufacturers list.", exception);
                errorProvider.SetError(comboBoxManufacturers, "Did you set the profile directory?");
                throw;
            }
        }

        private void SearchProfiles()
        {
            string manufacturerName = comboBoxManufacturers.Text;

            string[] searchWords = textBoxSearchPattern.Text.Split(' ');

            const string numericalRegEx = @"[0-9]";
            List<string> stringPatterns = new List<string>();

            // We match every search word for the pattern 'some chars'+'some digits'
            // The goal is to separate words from digits
            Match match;
            for (int k = 0; k < searchWords.Length; k++)
            {
                match = Regex.Match(searchWords[k], numericalRegEx);
                if (match.Success)
                {
                    // if the word does not start with digit
                    if (match.Index != 0)
                    {
                        // Split the word into two parts - alphabetical and numerical
                        // and add them to the patterns container separately
                        stringPatterns.Add(searchWords[k].Substring(0, match.Index));
                        stringPatterns.Add(searchWords[k].Substring(match.Index));
                    }
                    else // if it is a pure digit - add it to the patterns container
                    {
                        stringPatterns.Add(searchWords[k]);
                    }
                }
                else
                {
                    // if the word does not contain digits
                    stringPatterns.Add(searchWords[k]);
                }
            }

            XDocument profilesDb = XDocument.Load(_profilesCatalog);
            var query = from p in profilesDb.Element("Profiles").Elements("p")
                        select p.Value;

            if (!manufacturerName.Contains("All"))
            {
                query = query.Where(p => p.Contains(manufacturerName));
            }

            var q = profilesDb.Element("Profiles").Elements("p").Select(p => p.Value);
            for (int i = 0; i < stringPatterns.Count; i++)
            {
                int i1 = i;
                query = query.Where(p => p.IndexOf(stringPatterns[i1], StringComparison.OrdinalIgnoreCase) >= 0);
            }

            try
            {
                IList<string> profiles = query.ToList();
                if (profiles.Count == 0)
                {
                    listBoxProfiles.DataSource = new List<string> { "There are no profiles that match the search pattern." };
                }
                else
                {
                    listBoxProfiles.DataSource = profiles.Select(p => p.Replace(Settings.Default.FileServer, "")).ToList<string>();
                    //listBoxProfiles.DataSource = profiles.Select(p => p.Remove(0, Settings.Default.FileServer.Length + 1)).ToList();
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("Failed to populate Profiles list: " + ex.Message);
                errorProvider.SetError(buttonSearch, "Error during profiles listing.");
                _logger.Error("Failed to populate Profiles list: " , exception);
            }
            finally
            {
                profilesDb = new XDocument();
            }
        }

        private void UpdateProfiles()
        {
            try
            {

                if (!Directory.Exists(Settings.Default.FileServer))
                {
                    errorProvider.SetError(buttonProfilesDirectory, "Can not find profiles directory. Set correct profiles directory and click update profiles.");
                    return;
                }

                if (!Directory.Exists(_kojtoCadTempDir))
                {
                    errorProvider.SetError(buttonProfilesDirectory, "Can not find temp dir. Contact support.");
                    return;
                }

                XElement manufacturersXmlCatalog = new XElement(ManufacturersCatalogTitle);
                string[] manufacturersDirectories = Directory.GetDirectories(Settings.Default.FileServer);
                foreach (var manufacturersDirectory in manufacturersDirectories)
                {
                    manufacturersXmlCatalog.Add(new XElement("m", manufacturersDirectory.Remove(0, (Settings.Default.FileServer.Count()) + 1)));
                }

                if (File.Exists(_manufacturersCatalog))
                {
                    File.Delete(_manufacturersCatalog);
                }

                using (StreamWriter sw = File.CreateText(_manufacturersCatalog))
                {
                    sw.WriteLine(manufacturersXmlCatalog.ToString());
                }

                progressBar.Minimum = 0;
                progressBar.Maximum = manufacturersDirectories.Length;
                progressBar.Value = 0;
                progressBar.Step = 1;
                progressBar.Visible = true;
                XElement profileXmlCatalog = new XElement(ProfilesCatalogTitle);

                foreach (string manufacturersDirectory in manufacturersDirectories)
                {
                    progressBar.PerformStep();
                    string dir = Settings.Default.FileServer + "\\" + manufacturersDirectory.Remove(0, (Settings.Default.FileServer.Count()) + 1);
                    string[] files = Directory.GetFiles(Settings.Default.FileServer + "\\" + manufacturersDirectory.Remove(0, (Settings.Default.FileServer.Count()) + 1), "*.dwg", SearchOption.AllDirectories);
                    Array.Sort(files);

                    #region Insert profiles into DB

                    foreach (string file in files)
                    {
                        if (file.Contains("old") || file.Contains("OLD"))
                        {
                            continue;
                        }

                        profileXmlCatalog.Add(new XElement("p", file));
                    }

                    #endregion Insert profiles into DB
                }

                if (File.Exists(_profilesCatalog))
                {
                    File.Delete(_profilesCatalog);
                }

                using (StreamWriter sw = File.CreateText(_profilesCatalog))
                {
                    sw.WriteLine(profileXmlCatalog.ToString());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
            }
            finally
            {
                progressBar.Visible = false;
                try
                {
                    LoadManufacturers();
                }
                catch (Exception exception)
                {
                    _logger.Error(exception.Message);
                    _logger.Warn(exception.Message);
                    //errorProvider.SetError(comboBoxManufacturers, "Set profiles directory and click update profiles.");
                    //errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                    errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                    //errorProvider.SetError(buttonUpdateProfiles, "Set profiles directory and click update profiles.");
                }
            }
        }

        private void listBoxProfiles_DoubleClick(object sender, EventArgs e)
        {
            if (DwgIsSelected)
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            if (DwgIsSelected)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("There is no block to insert!");
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Settings.Default.FileServer))
            {
                errorProvider.SetError(buttonProfilesDirectory, "Set profiles directory and click update profiles.");
                return;
            }

            if (ValidateForm())
            {
                SearchProfiles();
            }
            //else
            //{
            //    errorProvider.SetError(buttonSearch,"Fill correct data in both search fields!", "Hint", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void buttonProfilesDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.SelectedPath = ((Directory.Exists(Settings.Default.FileServer))
                                                    ? Settings.Default.FileServer
                                                    : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));

            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (folderBrowserDialog.SelectedPath.Length == 3)
            {
                MessageBox.Show(
                    "You cannot select whole drive as root directory because the search will take too long.");
                return;
            }

            if (this.errorProvider.GetError(this.buttonProfilesDirectory).Length != 0)
            {
                this.errorProvider.SetError(this.buttonProfilesDirectory, String.Empty);
            }
            if (Settings.Default.FileServer != folderBrowserDialog.SelectedPath)
            {
                this.errorProvider.SetError(this.buttonUpdateProfiles, "Please, update selected profiles.");
            }

            Settings.Default.FileServer = folderBrowserDialog.SelectedPath;
            Settings.Default.Save();
        }

        private void buttonUpdateProfiles_Click(object sender, EventArgs e)
        {
            DialogResult usersChoice = MessageBox.Show("Updating the profiles database will take up to two minutes.", "Warning!", MessageBoxButtons.YesNo);
            if (usersChoice == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                UpdateProfiles();

                listBoxProfiles.DataSource = null;
                listBoxProfiles.Items.Clear();
            }
            Cursor.Current = Cursors.Default;

            if (errorProvider.GetError(buttonUpdateProfiles) != String.Empty)
            {
                errorProvider.SetError(buttonUpdateProfiles, String.Empty);
            }
        }

        private void comboBoxManufacturers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (errorProvider.GetError(comboBoxManufacturers) != string.Empty)
            {
                errorProvider.SetError(textBoxSearchPattern, String.Empty);
            }
            textBoxSearchPattern.Focus();
        }

        private void listBoxProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDwg = Settings.Default.FileServer + listBoxProfiles.Text;
            if (File.Exists(selectedDwg))
            {
                DwgIsSelected = true;
            }
            else
            {
                DwgIsSelected = false;
            }
            pictureBox.Image = acThumbnailReader.GetThumbnail(selectedDwg, false);
        }

        private void textBoxSearchPattern_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (errorProvider.GetError(textBoxSearchPattern) != string.Empty)
            {
                errorProvider.SetError(textBoxSearchPattern, String.Empty);
                errorProvider.Clear();
            }
            /* First we need a faster searching method!
            if(ValidateForm( ) )
            {
                LoadProfiles( );
            }
             * */
        }

        private bool ValidateForm()
        {
            if (!ValidateManufacturer())
            {
                return false;
            }

            if (!ValidateSearchPattern())
            {
                return false;
            }
            return true;
        }

        private bool ValidateSearchPattern()
        {
            if (textBoxSearchPattern.Text.Contains("'") || textBoxSearchPattern.Text.Contains("\""))
            {
                errorProvider.SetError(textBoxSearchPattern, "The symbols ' and \" are not allowed for search pattern.");
                return false;
            }

            string SearchPattern = textBoxSearchPattern.Text;
            if (SearchPattern.Length < 1 || SearchPattern == "Enter search pattern")
            {
                errorProvider.SetError(textBoxSearchPattern, "At least one symbol is required for search pattern.");
                return false;
            }
            return true;
        }

        private bool ValidateManufacturer()
        {
            string Manufacturer = comboBoxManufacturers.Text;
            if (Manufacturer.Length < 1)
            {
                errorProvider.SetError(comboBoxManufacturers, "Select manufacturer");
                return false;
            }
            return true;
        }

        private void ImportBlockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.lastSelectedManufacturer = comboBoxManufacturers.Text;
            Settings.Default.lastSelectedSearchPattern = textBoxSearchPattern.Text;
            Settings.Default.Save();
        }
    }
}