namespace KojtoCAD.Updater.Interfaces
{
    public interface IKojtoCadVersionValidator
    {
        bool IsValid(KojtoCadVersion version);

        bool IsValid(string version);
    }
}
