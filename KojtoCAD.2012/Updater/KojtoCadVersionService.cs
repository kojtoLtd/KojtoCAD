using KojtoCAD.Updater.Interfaces;
using KojtoCAD.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KojtoCAD.Updater
{
    public class KojtoCadVersionService : IKojtoCadVersionService
    {
        private readonly IKojtoCadVersionValidator _versionValidator;
        private readonly IInstallationPackageRepository _packageRepository;
        private readonly IUtilityClass _utilityClass;
        private readonly IKojtoCadVersionProvider _versionProvider;
        public KojtoCadVersionService(IKojtoCadVersionValidator versionValidator, 
            IInstallationPackageRepository packageRepository, 
            IUtilityClass utilityClass, 
            IKojtoCadVersionProvider versionProvider)
        {
            _versionValidator = versionValidator;
            _packageRepository = packageRepository;
            _utilityClass = utilityClass;
            _versionProvider = versionProvider;
        }

        public KojtoCadVersion GetLastReleasedVersion(KojtoCadVersion currentVersion)
        {
            if (!_versionValidator.IsValid(currentVersion))
            {
                throw new ArgumentException(nameof(currentVersion));
            }

            var availableVersions = _packageRepository.GetAvailablePackageVersions();

            var latest =
                availableVersions.OrderBy(x => x.Major)
                    .ThenBy(x => x.Year)
                    .ThenBy(x => x.ElapsedDays)
                    .ThenBy(x => x.Revision)
                    .LastOrDefault();
            return latest;
        }

        public async Task<KojtoCadVersion> GetLastReleasedVersionAsync(KojtoCadVersion currentVersion)
        {
            return await Task.Run(() => GetLastReleasedVersion(currentVersion));
        }

        public KojtoCadVersion GetInstalledProductVersion()
        {
            var versionText = _utilityClass.GetCurrentAssemblyFileVersion();
            var version = _versionProvider.GetVersionFromText(versionText);
            if (!_versionValidator.IsValid(version))
            {
                throw new Exception("Invalid product version!");
            }
            return version;
        }

        public IEnumerable<Tuple<KojtoCadVersion,string>> GetPreviousInstalledVersions(string installDir, KojtoCadVersion currentVersion)
        {
            var dir = new DirectoryInfo(installDir);
            if (!dir.Exists)
            {
                throw new ArgumentException(nameof(installDir));
            }

            return
                dir.EnumerateDirectories()
                    .Where(x => _versionValidator.IsValid(x.Name))
                    .Select(x => Tuple.Create(_versionProvider.GetVersionFromText(x.Name), x.FullName))
                    .Where(x => x.Item1 < currentVersion);
        }

    }
}
