using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KojtoCAD.Updater.Interfaces
{
    public interface IKojtoCadVersionService
    {
        KojtoCadVersion GetLastReleasedVersion(KojtoCadVersion currentVersion);

        Task<KojtoCadVersion> GetLastReleasedVersionAsync(KojtoCadVersion currentVersion);
        KojtoCadVersion GetInstalledProductVersion();

        IEnumerable<Tuple<KojtoCadVersion, string>> GetPreviousInstalledVersions(string installDir, KojtoCadVersion currentVersion);
    }
}
