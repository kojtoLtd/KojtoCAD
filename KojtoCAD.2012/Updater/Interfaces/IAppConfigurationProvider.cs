namespace KojtoCAD.Updater.Interfaces
{
    public interface IAppConfigurationProvider
    {
        string GetBlobConnectionString();

        string GetBlobContainerName();

        string GetKojtoCadVirtualDirectoryName();

        string GetProgramFilesDir();

        string GetKojtoCadPluginDir();

        bool WebTrackerIsEnabled();
    }
}
