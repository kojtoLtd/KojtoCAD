using System.Windows.Forms;

namespace KojtoCAD.Utilities.Interfaces
{
    public interface IUtilityClass
    {
        string GetCurrentAssemblyFileVersion();

        string GetApplicationName();

        DialogResult ShowDialog(Form dialog);
    }
}
