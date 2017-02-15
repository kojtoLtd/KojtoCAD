using System.Collections.Generic;
using KojtoCAD.Updater;
using KojtoCAD.Updater.Interfaces;
using Xunit;

namespace KojtoCAD.UnitTests.UpdaterTests
{
    public class VersionValidatorTests
    {
        private readonly IKojtoCadVersionValidator _validator;

        public VersionValidatorTests()
        {
            _validator = new KojtoCadVersionValidator();
        }

        public static IEnumerable<object[]> GetDataObject()
        {
            yield return new object[] {-1, 2016, 2, 2, false};// invalid major
            yield return new object[] { 0, 2011, 2, 2, false };// invalid year
            yield return new object[] { 0, 0, 2, 2, false };// invalid year
            yield return new object[] { 0, -1, 2, 2, false };// invalid year
            yield return new object[] { 0, 2012, 0, 2, false };// invalid elapsed days
            yield return new object[] { 0, 2012, 367, 2, false };// invalid elapsed days
            yield return new object[] { 0, 2013, 366, 2, false };// invalid elapsed days
            yield return new object[] { 0, 2013, 0, 2, false };// invalid elapsed days
            yield return new object[] { 0, 2013, -2, 2, false };// invalid elapsed days
            yield return new object[] { 0, 2013, 150, 0, false };// invalid revision
            yield return new object[] { 0, 2013, 150, -5, false };// invalid revision
            yield return new object[] { 1, 2017, 365, 2, true };// valid
            yield return new object[] { 1, 2016, 365, 1, true };// valid
            yield return new object[] { 1, 2016, 366, 1, true };// valid
        }
        [Theory]
        [MemberData(nameof(GetDataObject))]
        public void ValidateKojtoCadVersionObject(int major, int minor, int buildNumber, int revisionNumber, bool result)
        {
            var version = new KojtoCadVersion
            {
                Major = major,
                Year = minor,
                ElapsedDays = buildNumber,
                Revision = revisionNumber
            };
            var res = _validator.IsValid(version);
            Assert.Equal(result, res);
        }

        public static IEnumerable<object[]> GetDataText()
        {
            yield return new object[] {"-1.2016.2.2", false}; // invalid major
            yield return new object[] { "h.2016.2.2", false }; // invalid major
            yield return new object[] {"0.2011.2.2", false}; // invalid year
            yield return new object[] {"0.0.2.2", false}; // invalid year
            yield return new object[] {"0.-1.2.2", false}; // invalid year
            yield return new object[] { "0.201t.2.2", false }; // invalid year
            yield return new object[] {"0.2012.0.2", false}; // invalid elapsed days
            yield return new object[] {"0.2012.367.2", false}; // invalid elapsed days
            yield return new object[] {"0.2013.366.2", false}; // invalid elapsed days
            yield return new object[] {"0.2013.0.2", false}; // invalid elapsed days
            yield return new object[] {"0.2013.-2.2", false}; // invalid elapsed days
            yield return new object[] { "0.2013.d.2", false }; // invalid elapsed days
            yield return new object[] {"0.2013.150.0", false}; // invalid revision
            yield return new object[] {"0.2013.150.-5", false}; // invalid revision
            yield return new object[] { "0.2013.150.5s", false }; // invalid revision
            yield return new object[] {"1.2017.365.2", true}; // valid
            yield return new object[] {"1.2016.365.1", true}; // valid
            yield return new object[] {"1.2016.366.1", true}; // valid
        }

        [Theory]
        [MemberData(nameof(GetDataText))]
        public void ValidateKojtoCadVersionText(string version, bool result)
        {
            var res = _validator.IsValid(version);
            Assert.Equal(result, res);
        }
    }
}
