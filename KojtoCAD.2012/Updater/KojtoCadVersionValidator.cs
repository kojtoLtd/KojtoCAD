using System;
using System.Linq;
using System.Text.RegularExpressions;
using KojtoCAD.Updater.Interfaces;

namespace KojtoCAD.Updater
{
    public class KojtoCadVersionValidator : IKojtoCadVersionValidator
    {
        // Example: 1.2016.245.2

        private const int YearMinValue = 2012;
        private const string VersionRegex = @"^[0-9]{1,2}\.2[0-9]{3}\.[0-9]{1,3}\.[0-9]+$";

        public bool IsValid(KojtoCadVersion version)
        {
            return version != null && ValidateMajor(version.Major.ToString()) && ValidateYear(version.Year.ToString()) &&
                   ValidateElapsedDays(version.ElapsedDays.ToString(), version.Year) &&
                   ValidateRevision(version.Revision.ToString());
        }

        public bool IsValid(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            if (!Regex.IsMatch(version, VersionRegex))
            {
                return false;
            }

            var split = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            return ValidateMajor(split[0]) && ValidateYear(split[1]) &&
                   ValidateElapsedDays(split[2], int.Parse(split[1])) &&
                   ValidateRevision(split[3]);
        }

        private bool ValidateMajor(string major)
        {
            int x;
            var parse = int.TryParse(major, out x);
            return parse && x >= 0;
        }

        private bool ValidateYear(string year)
        {
            int y;
            var parse = int.TryParse(year, out y);
            return parse && y >= YearMinValue;
        }

        private bool ValidateElapsedDays(string elapsedDays, int year)
        {
            int x;
            var daysParse = int.TryParse(elapsedDays, out x);
            return daysParse && x >= 1 && x <= 366 && (x != 366 || DateTime.IsLeapYear(year));
        }

        private bool ValidateRevision(string revision)
        {
            int x;
            var versionIsValid = int.TryParse(revision, out x);
            return versionIsValid && x > 0;
        }
    }
}
