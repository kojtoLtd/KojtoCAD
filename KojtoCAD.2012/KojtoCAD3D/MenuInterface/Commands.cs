using System.Windows.Forms;
using KojtoCAD.IoC;
using KojtoCAD.Ui.Interfaces;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

#else
using Bricscad.ApplicationServices;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.MenuInterface.Commands))]

namespace KojtoCAD.KojtoCAD3D.MenuInterface
{
    public class Commands
    {
        [CommandMethod("KojtoCAD_3D", "KCAD_MENU", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/kcad_menu.htm", "")]
        public void KojtoCAD_3D_MAIN_MENU()
        {
            
            
#if bcad 
            MessageBox.Show("Currently not available for BricsCAD!", "W A R N I N G !", MessageBoxButtons.OK);
            return;
            var schemaProvider = ContainerRegistrar.Container.Resolve<IMenuSchemaProvider>();
            new BcadMenuGenerator(schemaProvider).GenerateUiFile();
#else
            new AcadMenuGenerator().GenerateUiFile();
#endif
        }

        private void LoadMyCui(string cuiFile)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            object oldCmdEcho = Application.GetSystemVariable("CMDECHO");

            Application.SetSystemVariable("CMDECHO", 0);
            doc.SendStringToExecute("(command \"_.CUILOAD\" \"" + cuiFile + "\")(setvar \"CMDECHO\" " + oldCmdEcho + ")(princ) ", false, false, false);
        }
    }
}
