namespace KojtoCAD.Updater.Interfaces
{
    public interface IKojtoCadVersionProvider
    {
        KojtoCadVersion GetVersionFromText(string versionText);
    }
}
