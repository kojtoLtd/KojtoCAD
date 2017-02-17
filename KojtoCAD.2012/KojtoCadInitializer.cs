using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using KojtoCAD.IoC;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Updater.Interfaces;
using Exception = System.Exception;
#if !bcad
using Autodesk.AutoCAD.Runtime;
#else
using Teigha.Runtime;
#endif

[assembly: ExtensionApplication(typeof(KojtoCAD.KojtoCadInitializer))]

namespace KojtoCAD
{
    public class KojtoCadInitializer : IExtensionApplication
    {
        void IExtensionApplication.Initialize()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

            ContainerRegistrar.SetupContainer();

            var uiGenerator = ContainerRegistrar.Container.Resolve<IUiGenerator>();

            try
            {
                uiGenerator.GenerateUi(false);
            }
            catch (Exception e)
            {
                // TODO : Application insights
                //MessageBox.Show(e.Message);
            }

            ContainerRegistrar.Container.Release(uiGenerator);

            IKojtoCadUpdater updater;
            try
            {
                updater = ContainerRegistrar.Container.Resolve<IKojtoCadUpdater>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            try
            {
                updater.UpdateKojtoCad();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CurrentDomainOnFirstChanceException(object sender,
            FirstChanceExceptionEventArgs firstChanceExceptionEventArgs)
        {
            //var exception = firstChanceExceptionEventArgs.Exception;
            //var methodname = new StackTrace(exception).GetFrame(0).GetMethod().Name;
            var thisasm = Assembly.GetExecutingAssembly();
            if (thisasm.GetName().Name.ToLower().Contains("kojto"))
            {
                var exception = firstChanceExceptionEventArgs.Exception;
            }
        }


        void IExtensionApplication.Terminate()
        {
            ContainerRegistrar.DisposeContainer();
        }
    }
}