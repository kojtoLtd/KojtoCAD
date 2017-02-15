using System.Xml;

namespace KojtoCAD.Ui.Interfaces
{
    public interface IMenuSchemaProvider
    {
        XmlDocument GetMenuSchemaFile();

        XmlDocument GetKojto3DMenuSchema();
    }
}