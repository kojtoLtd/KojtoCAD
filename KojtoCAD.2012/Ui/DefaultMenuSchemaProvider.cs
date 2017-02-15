using System.IO;
using System.Reflection;
using System.Xml;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Utilities;

namespace KojtoCAD.Ui
{
    public class DefaultMenuSchemaProvider : IMenuSchemaProvider
    {
        private readonly ILogger _logger;

        public DefaultMenuSchemaProvider(ILogger logger)
        {
            _logger = logger;
        }

        public XmlDocument GetMenuSchemaFile()
        {
            string executingAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);         
            var kojtoCadMenuDefinition = new XmlDocument();          
            string menuTemplateFile = Path.Combine(executingAssemblyLocation, Settings.Default.templatesDir, Settings.Default.appName + ".xml");
            try
            {
                kojtoCadMenuDefinition.Load(menuTemplateFile);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.Error("Menu shcema file failed to load.", fileNotFoundException );
                throw;
            }
            return kojtoCadMenuDefinition;
        }

        public XmlDocument GetKojto3DMenuSchema()
        {
            var menuSchema = DirectoriesAndFiles.KCad3DMenuSchema;
            var menuDefinition = new XmlDocument();
            try
            {
                menuDefinition.Load(menuSchema);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.Error("Menu shcema file failed to load.", fileNotFoundException);
                throw;
            }
            return menuDefinition;
        }
    }
}