using System.Threading.Tasks;

namespace KojtoCAD.Updater.Interfaces
{
    public interface IAutoloaderSettingsService
    {
        void EditSettingsSoTheNewVersionWillBeLoadedOnNextStartup(string newVersionDir);

        Task EditSettingsSoTheNewVersionWillBeLoadedOnNextStartupAsync(string newVersionDir);

        void RevertOldValues();
    }
}
