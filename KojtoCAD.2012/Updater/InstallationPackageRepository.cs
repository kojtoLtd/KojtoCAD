using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KojtoCAD.Updater.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace KojtoCAD.Updater
{
    public class InstallationPackageRepository : IInstallationPackageRepository
    {
        private readonly IKojtoCadVersionProvider _versionProvider;
        private readonly IAppConfigurationProvider _configurationProvider;
        private readonly IKojtoCadVersionValidator _versionValidator;

        public InstallationPackageRepository(IKojtoCadVersionProvider versionProvider, 
            IAppConfigurationProvider configurationProvider, IKojtoCadVersionValidator versionValidator)
        {
            _versionProvider = versionProvider;
            _configurationProvider = configurationProvider;
            _versionValidator = versionValidator;
        }

        public IEnumerable<KojtoCadVersion> GetAvailablePackageVersions()
        {
            var storageAccount = CloudStorageAccount.Parse(_configurationProvider.GetBlobConnectionString());
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_configurationProvider.GetBlobContainerName());
            var dirWithVersions = container.GetDirectoryReference(_configurationProvider.GetKojtoCadVirtualDirectoryName());
            return
                dirWithVersions.ListBlobs()
                    .Select(x => x as CloudBlobDirectory)
                    .Where(x => x != null)
                    .Select(x => ExtractVersionFromDirectoryName(x.Prefix))
                    .Where(_versionValidator.IsValid)
                    .Select(_versionProvider.GetVersionFromText);
        }

        public async Task<IEnumerable<KojtoCadVersion>> GetAvailablePackageVersionsAsync()
        {
            return await Task.Run(() => GetAvailablePackageVersions());
        }

        public void DownloadPackage(KojtoCadVersion packageVersion, string destinationDir)
        {
            var storageAccount = CloudStorageAccount.Parse(_configurationProvider.GetBlobConnectionString());
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_configurationProvider.GetBlobContainerName());
            var dirWithVersions = container.GetDirectoryReference(_configurationProvider.GetKojtoCadVirtualDirectoryName());
            var newVersionDirs = dirWithVersions.ListBlobs().Select(x => x as CloudBlobDirectory).ToList();

            var newVersionDir =
                newVersionDirs.Where(x => x != null)
                    .Select(x => new Tuple<CloudBlobDirectory, string>(x, ExtractVersionFromDirectoryName(x.Prefix)))
                    .Where(x => _versionValidator.IsValid(x.Item2))
                    .Select(
                        x =>
                            new Tuple<CloudBlobDirectory, KojtoCadVersion>(x.Item1,
                                _versionProvider.GetVersionFromText(x.Item2)))
                    .FirstOrDefault(x => packageVersion.Equals(x.Item2));
            
            if (newVersionDir == null)
            {
                throw new ArgumentException("packageVersion");
            }

            var downloadTasks = newVersionDir.Item1.ListBlobs(true).Select(x => x as CloudBlockBlob).Where(x => x != null);
            foreach (var cloudBlockBlob in downloadTasks)
            {
                var relativePath = cloudBlockBlob.Name.Substring(newVersionDir.Item1.Prefix.Length);
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

                var storageAccount = CloudStorageAccount.Parse(_configurationProvider.GetBlobConnectionString());
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_configurationProvider.GetBlobContainerName());
                var dirWithVersions =
                    container.GetDirectoryReference(_configurationProvider.GetKojtoCadVirtualDirectoryName());

                var newVersionDirs = dirWithVersions.ListBlobs().Select(x => x as CloudBlobDirectory).ToList();
                var newVersionDir =
                    newVersionDirs.Where(x => x != null)
                        .Select(x => new Tuple<CloudBlobDirectory, string>(x, ExtractVersionFromDirectoryName(x.Prefix)))
                        .Where(x => _versionValidator.IsValid(x.Item2))
                        .Select(
                            x =>
                                new Tuple<CloudBlobDirectory, KojtoCadVersion>(x.Item1,
                                    _versionProvider.GetVersionFromText(x.Item2)))
                        .FirstOrDefault(x => packageVersion.Equals(x.Item2));

                if (newVersionDir == null)
                {
                    throw new ArgumentException("packageVersion");
                }
                var downloadTasks =
                    newVersionDir.Item1.ListBlobs(true).Select(x => x as CloudBlockBlob).Where(x => x != null).ToArray();
                var count = downloadTasks.Length;
                for (var i = 0; i < downloadTasks.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var cloudBlockBlob = downloadTasks[i];
                    var relativePath = cloudBlockBlob.Name.Substring(newVersionDir.Item1.Prefix.Length);
                    var file = Path.Combine(destinationDir, relativePath);

                    new FileInfo(file).Directory.Create();
                    // test area

                    //var asdas = cloudBlockBlob.DownloadToFileAsync("", FileMode.Append);

                    // ***********************
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
    }
}
