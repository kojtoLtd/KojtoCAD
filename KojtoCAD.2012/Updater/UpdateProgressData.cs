namespace KojtoCAD.Updater
{
    public class UpdateProgressData
    {
        // Download task
        public int FilesCount { get; set; }
        public int CurrentFile { get; set; }

        // Copy task
        public bool CopyCompleted { get; set; }

        // Remove leftovers
        public bool RemoveTemp { get; set; }

        // Edit Autoloader mechanism
        public bool EditAutoloaderSettingsCompleted { get; set; }
    }
}
