namespace KojtoCAD.Ui.Interfaces
{
    public interface IIconManager
    {
        string GetIconFile(string iconName, IconSize iconSize);
        void DeleteIconFile(string iconFileFullPath);
    }

    public enum IconSize
    {
        Small,
        Medium,
        Large
    }
}
