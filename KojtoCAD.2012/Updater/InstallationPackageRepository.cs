using KojtoCAD.Persistence.Interfaces;
using KojtoCAD.Updater.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KojtoCAD.Updater
{
    public class InstallationPackageRepository : IInstallationPackageRepository
    {
        private readonly IKojtoCadVersionProvider _versionProvider;
        private readonly IAppConfigurationProvider _configurationProvider;
        private readonly IKojtoCadVersionValidator _versionValidator;
        private readonly IBlobRepository _blobRepository;

        public InstallationPackageRepository(IKojtoCadVersionProvider versionProvider, 
            IAppConfigurationProvider configurationProvider, IKojtoCadVersionValidator versionValidator,
             IBlobRepository blobRepository)
        {
            _versionProvider = versionProvider;
            _configurationProvider = configurationProvider;
            _versionValidator = versionValidator;
            _blobRepository = blobRepository;
        }

        public IEnumerable<KojtoCadVersion> GetAvailablePackageVersions()
        {
            var containerUri = new Uri(_configurationProvider.GetBlobContainerUri());
            var directoryHoldingNewVersions = _configurationProvider.GetKojtoCadVirtualDirectoryName();
            var newVersions = _blobRepository.GetDirectories(containerUri, directoryHoldingNewVersions);
            return
                newVersions.Select(x => ExtractVersionFromDirectoryName(x.Prefix))
                .Where(_versionValidator.IsValid).Select(_versionProvider.GetVersionFromText);
        }

        public async Task<IEnumerable<KojtoCadVersion>> GetAvailablePackageVersionsAsync()
        {
            return await Task.Run(() => GetAvailablePackageVersions());
        }

        public void DownloadPackage(KojtoCadVersion packageVersion, string destinationDir)
        {
            var containerUri = new Uri(_configurationProvider.GetBlobContainerUri());
            var directoryHoldingNewVersions = _configurationProvider.GetKojtoCadVirtualDirectoryName();
            var specifiedVersionDir = Path.Combine(directoryHoldingNewVersions, GetDirectoryCorrespondingToVersion(packageVersion));
            var blobs = _blobRepository.GetBlobsFromDirectory(containerUri, specifiedVersionDir);

            foreach (var cloudBlockBlob in blobs)
            {
                var relativePath = cloudBlockBlob.Name.Substring(specifiedVersionDir.Length);
                var file = Path.Combine(destinationDir, relativePath);

                new FileInfo(file).Directory.Create();

                cloudBlockBlob.DownloadToFile(file, FileMode.Create);
            }
        }

        public async Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir,
            IProgress<UpdateProgressData> progress)
        {
            await DownloadPackageAsync(packageVersion, destinationDir, progress, CancellationToken.None);
        }

        public async Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir, IProgress<UpdateProgressData> progress, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var containerUri = new Uri(_configurationProvider.GetBlobContainerUri());
                var directoryHoldingNewVersions = _configurationProvider.GetKojtoCadVirtualDirectoryName();
                var specifiedVersionDir = Path.Combine(directoryHoldingNewVersions, GetDirectoryCorrespondingToVersion(packageVersion));
                var blobs = _blobRepository.GetBlobsFromDirectory(containerUri, specifiedVersionDir);

                var count = blobs.Length;
                for (var i = 0; i < blobs.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var cloudBlockBlob = blobs[i];
                    // make the same directory structure using relative paths
                    var relativePath = cloudBlockBlob.Name.Substring(specifiedVersionDir.Length+1);
                    var file = Path.Combine(destinationDir, relativePath);

                    new FileInfo(file).Directory.Create();
                    cloudBlockBlob.DownloadToFile(file, FileMode.Create);
                    progress.Report(new UpdateProgressData
                    {
                        CurrentFile = i + 1,
                        FilesCount = count
                    });
                }
            }, cancellationToken);
        }

        public async Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir)
        {
            await Task.Run(() => DownloadPackage(packageVersion, destinationDir));
        }

        private string ExtractVersionFromDirectoryName(string directoryName)
        {
            return directoryName.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)[1];
        }

        private string GetDirectoryCorrespondingToVersion(KojtoCadVersion packageVersion)
        {
            return packageVersion.ToString();
        }
    }
}
