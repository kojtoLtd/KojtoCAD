namespace KojtoCAD.Updater.Interfaces
{
    public interface IAppConfigurationProvider
    {
        string GetBlobContainerUri();

        string GetBlobContainerName();

        string GetKojtoCadVirtualDirectoryName();

        string GetProgramFilesDir();

        string GetKojtoCadPluginDir();

        bool WebTrackerIsEnabled();
    }
}
