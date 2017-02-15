using KojtoCAD.Updater;
using KojtoCAD.Updater.Interfaces;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace KojtoCAD.UnitTests.UpdaterTests
{
    public class KojtoCadVersionTests
    {
        private readonly IKojtoCadVersionProvider _versionProvider;
        public KojtoCadVersionTests()
        {
            _versionProvider = new KojtoCadVersionProvider(new KojtoCadVersionValidator());
        }

        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] { "02.2016.01.1", "01.2016.01.1", true };
            yield return new object[] { "02.2016.01.1", "01.2017.355.5", true };
            yield return new object[] { "01.2016.01.1", "01.2015.250.5", true };
            yield return new object[] { "02.2016.02.1", "02.2016.01.50", true };
            yield return new object[] { "02.2016.02.10", "02.2016.02.9", true };
            yield return new object[] { "02.2016.02.1", "02.2016.02.1", false };
            yield return new object[] { "02.2016.02.1", "02.2016.02.2", false };
            yield return new object[] { "02.2016.02.1", "02.2016.03.1", false };
        }
        [Theory]
        [MemberData(nameof(GetData))]
        public void VersionComparisonTests(string firstText, string secondTest, bool result)
        {
            var first = _versionProvider.GetVersionFromText(firstText);
            var second = _versionProvider.GetVersionFromText(secondTest);
            Assert.Equal(first > second, result);
        }

        public static IEnumerable<object[]> EqualityDate()
        {
            yield return new object[] {"02.2016.01.1", "01.2016.01.1", false};
            yield return new object[] {"01.2016.01.1", "01.2015.01.1", false};
            yield return new object[] {"01.2016.02.1", "01.2016.01.1", false};
            yield return new object[] {"01.2016.01.1", "01.2016.01.1", true};
            yield return new object[] {"01.2016.01.1", "1.2016.1.1", true};
        }

        [Theory]
        [MemberData(nameof(EqualityDate))]
        public void VersionEqualityTests(string firstText, string secondTest, bool result)
        {
            var first = _versionProvider.GetVersionFromText(firstText);
            var second = _versionProvider.GetVersionFromText(secondTest);
            Assert.Equal(first == second, result);
        }
    }
}
