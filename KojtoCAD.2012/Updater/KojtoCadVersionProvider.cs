using KojtoCAD.Updater.Interfaces;
using System;

namespace KojtoCAD.Updater
{
    public class KojtoCadVersionProvider: IKojtoCadVersionProvider
    {
        private readonly IKojtoCadVersionValidator _versionValidator;

        public KojtoCadVersionProvider(IKojtoCadVersionValidator versionValidator)
        {
            _versionValidator = versionValidator;
        }

        public KojtoCadVersion GetVersionFromText(string versionText)
        {
            if (!_versionValidator.IsValid(versionText))
            {
                throw new ArgumentException(nameof(versionText));
            }

            // old 
            // KojtoCAD2013.1.0.0.0
            // 2016.08.10.3

            // new 
            // 1.2017.365.3
            var split = versionText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var major = int.Parse(split[0]);
            var year = int.Parse(split[1]);
            var elapsedDays = int.Parse(split[2]);
            var revision = int.Parse(split[3]);
            var version = new KojtoCadVersion { Revision = revision, ElapsedDays = elapsedDays, Year = year, Major = major };
            return version;
        }
    }
}
