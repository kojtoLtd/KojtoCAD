using KojtoCAD.Updater.Interfaces;
using KojtoCAD.Updater.Ui;
using KojtoCAD.Utilities.Interfaces;
using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KojtoCAD.Updater
{
    public class KojtoCadUpdater : IKojtoCadUpdater
    {
        private readonly IInstallationPackageRepository _packageRepository;
        private readonly IKojtoCadVersionService _versionService;
        private readonly IUtilityClass _utilityClass;
        private readonly IFileService _fileService;
        private readonly IAppConfigurationProvider _appConfigurationProvider;
        private readonly IAutoloaderSettingsService[] _autoloaderSettingsServices;

        public KojtoCadUpdater(
            IInstallationPackageRepository packageRepository,
            IKojtoCadVersionService versionProvider,
            IUtilityClass utilityClass,
            IFileService fileService,
            IAppConfigurationProvider appConfigurationProvider,
            IAutoloaderSettingsService[] autoloaderSettingsServices)
        {
            _packageRepository = packageRepository;
            _versionService = versionProvider;
            _utilityClass = utilityClass;
            _fileService = fileService;
            _appConfigurationProvider = appConfigurationProvider;
            _autoloaderSettingsServices = autoloaderSettingsServices;
        }

        public async Task UpdateKojtoCad()
        {
            var oldContext = SynchronizationContext.Current;
            SynchronizationContext context = oldContext;
            if (oldContext == null)
            {
                context = new WindowsFormsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
            }

            var currentVersion = _versionService.GetInstalledProductVersion();

            // last available revision
            Task<KojtoCadVersion> lastVersionTask;
            try
            {
                lastVersionTask = _versionService.GetLastReleasedVersionAsync(currentVersion);
                await lastVersionTask;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }

            
            var lastVersion = lastVersionTask.Result;
            if (lastVersion == null)
            {
                MessageBox.Show("No new versions");
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }
            if (lastVersion <= currentVersion)
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }

            // changes Sync context from AutoCadSyncContext/WindowsFormsSyncContext to new SyncronizationContext()
            if (_utilityClass.ShowDialog(new UpdateDialog(currentVersion.ToString(), lastVersion.ToString())) !=
                DialogResult.OK)
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }

            if (!IsAdministrator())
            {
                var message = "In order to update KojtoCAD you need to start your CAD software as Administrator. " +
                              "Right-click on the CAD icon and select the 'Run as Administrator' option. " +
                              "In case this option does not exist contact your IT administrator.";
                MessageBox.Show(message, "E R R O R !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }

            SynchronizationContext.SetSynchronizationContext(context);

            var progressDialog = new UpdateProgressDialog();
            progressDialog.Show();

            var cancellationToken = progressDialog.CancellationTokenSource.Token;

            string newVersionDir = null;
            try
            {
                // remove old versions
                var bundleDir = _appConfigurationProvider.GetKojtoCadPluginDir();
                var previousVersions = _versionService.GetPreviousInstalledVersions(bundleDir, currentVersion);
                foreach (var previousVersion in previousVersions)
                {
                    await _fileService.DeleteDirectoryAsync(previousVersion.Item2, true, cancellationToken);
                }

                // download files
                var tempCopy = Path.Combine(_fileService.GetUsersTempDir(), lastVersion.ToString());
                await _packageRepository.DownloadPackageAsync(lastVersion, tempCopy, progressDialog.Progress,
                    cancellationToken);

                // copy new version
                newVersionDir = Path.Combine(bundleDir, lastVersion.ToString());
                await _fileService.CopyDirectoryAsync(tempCopy, newVersionDir, cancellationToken);
                progressDialog.Progress.Report(new UpdateProgressData {CopyCompleted = true});

                // remove temp files
                await _fileService.DeleteDirectoryAsync(tempCopy, true, cancellationToken);
                progressDialog.Progress.Report(new UpdateProgressData {RemoveTemp = true});

                cancellationToken.ThrowIfCancellationRequested();

                // edit PackageContents.xml file and Registry value
                foreach (var autoloaderSettingsService in _autoloaderSettingsServices)
                {
                    await autoloaderSettingsService.EditSettingsSoTheNewVersionWillBeLoadedOnNextStartupAsync(
                        newVersionDir);
                }

                progressDialog.Progress.Report(new UpdateProgressData { EditAutoloaderSettingsCompleted = true });
            }
            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                {
                    MessageBox.Show(e.Message, "E R R O R !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (Directory.Exists(newVersionDir))
                {
                    try
                    {
                        await _fileService.DeleteDirectoryAsync(newVersionDir, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "E R R O R !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                foreach (var autoloaderSettingsService in _autoloaderSettingsServices)
                {
                    autoloaderSettingsService.RevertOldValues();
                }

                // in case of exception there will be no background threads working 
                // and we can safely dispose cancellation token source
                progressDialog.Close();

                SynchronizationContext.SetSynchronizationContext(oldContext);
                return;
            }

            // revert synchronization context
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        private bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}