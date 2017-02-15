using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KojtoCAD.Updater.Interfaces
{
    public interface IInstallationPackageRepository
    {
        IEnumerable<KojtoCadVersion> GetAvailablePackageVersions();

        Task<IEnumerable<KojtoCadVersion>> GetAvailablePackageVersionsAsync();
        void DownloadPackage(KojtoCadVersion packageVersion, string destinationDir);

        Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir);

        Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir, IProgress<UpdateProgressData> progress);
        Task DownloadPackageAsync(KojtoCadVersion packageVersion, string destinationDir, IProgress<UpdateProgressData> progress, CancellationToken cancellationToken);
    }
}
